using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using HtmlAgilityPack;
using IdentityServer4.Extensions;
using LC.Crawler.BackOffice.Articles;
using LC.Crawler.BackOffice.DataSources;
using LC.Crawler.BackOffice.Medias;
using Volo.Abp.Domain.Services;
using LC.Crawler.BackOffice.Categories;
using LC.Crawler.BackOffice.Enums;
using LC.Crawler.BackOffice.Extensions;
using Volo.Abp.Auditing;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using WordPressPCL;
using WordPressPCL.Models;
using WordPressPCL.Utility;
using Guid = System.Guid;
using WordpresCategory = WordPressPCL.Models.Category;

namespace LC.Crawler.BackOffice.Wordpress;

public class WordpressManagerAladin : DomainService
{
    private readonly IArticleAladinRepository _articleAladinRepository;
    private readonly IMediaAladinRepository _mediaAladinRepository;
    private readonly ICategoryAladinRepository _categoryAladinRepository;
    private readonly IDataSourceRepository _dataSourceRepository;
    private DataSource _dataSource;
    private readonly WordpressManagerBase _wordpressManagerBase;
    private readonly IAuditingManager _auditingManager;

    private readonly DataSourceManager _dataSourceManager;

    public WordpressManagerAladin(IArticleAladinRepository articleAladinRepository,
        IMediaAladinRepository mediaAladinRepository,
        IDataSourceRepository dataSourceRepository,
        ICategoryAladinRepository categoryAladinRepository,
        WordpressManagerBase wordpressManagerBase,
        IAuditingManager auditingManager,
        DataSourceManager dataSourceManager)
    {
        _articleAladinRepository = articleAladinRepository;
        _mediaAladinRepository = mediaAladinRepository;
        _dataSourceRepository = dataSourceRepository;
        _categoryAladinRepository = categoryAladinRepository;
        _wordpressManagerBase = wordpressManagerBase;
        _auditingManager = auditingManager;
        _dataSourceManager = dataSourceManager;
    }

    public async Task DoSyncPostAsync(int songay = 3)
    {
        // get data source
        _dataSource = await _dataSourceRepository.GetAsync(x => x.Url.Contains(PageDataSourceConsts.AladinUrl));

        // update re-sync status
        await _dataSourceManager.DoUpdateSyncStatus(_dataSource.Id, PageSyncStatusType.SyncArticle, PageSyncStatus.InProgress);

        // get article ids
        var limitDate = DateTime.UtcNow.AddDays(-songay);
        var articleIds = (await _articleAladinRepository.GetQueryableAsync())
            .Where(x => x.DataSourceId == _dataSource.Id && x.ExternalId == null && x.Content != null && x.CreatedAt > limitDate).ToList().OrderByDescending(x => x.CreationTime)
            .Select(x => x.Id).ToList();

        // get all tags
        var wpTags = await _wordpressManagerBase.GetAllTags(_dataSource);

        // sync article to wp
        foreach (var articleId in articleIds)
        {
            using var auditingScope = _auditingManager.BeginScope();

            try
            {
                
                var articleNav = await _articleAladinRepository.GetWithNavigationPropertiesAsync(articleId);

                var featureMedia = await _wordpressManagerBase.PostMediaAsync(_dataSource, articleNav.Media);
                await _wordpressManagerBase.PostMediasAsync(_dataSource, articleNav);

                if (articleNav.Media is not null)
                {
                    await _mediaAladinRepository.UpdateAsync(articleNav.Media, true);
                }

                if (articleNav.Medias.IsNotNullOrEmpty())
                {
                    await _mediaAladinRepository.UpdateManyAsync(articleNav.Medias, true);
                }

                var post = await _wordpressManagerBase.DoSyncPostAsync(_dataSource, articleNav, wpTags, featureMedia);
                if (post is not null)
                {
                    var article = await _articleAladinRepository.GetAsync(articleId);
                    article.ExternalId = post.Id.To<int>();
                    article.LastSyncedAt = DateTime.UtcNow;
                    await _articleAladinRepository.UpdateAsync(article, true);
                    //await CheckFormatEntity(article);
                }
            }
            catch (Exception ex)
            {
                //Add exceptions
                _wordpressManagerBase.LogException(_auditingManager.Current.Log,
                    ex,
                    $"{articleId}",
                    PageDataSourceConsts.AladinUrl,
                    "DoSyncPostAsync");
            }
            finally
            {
                //Always save the log
                await auditingScope.SaveAsync();
            }
        }

        // update re-sync status
        await _dataSourceManager.DoUpdateSyncStatus(_dataSource.Id, PageSyncStatusType.SyncArticle, PageSyncStatus.Completed);
    }

    public async Task DoReSyncPostAsync()
    {
        // get data source
        _dataSource = await _dataSourceRepository.GetAsync(x => x.Url.Contains(PageDataSourceConsts.AladinUrl));
        if (_dataSource is not { ShouldReSyncArticle: true })
        {
            return;
        }

        // update re-sync status
        await _dataSourceManager.DoUpdateSyncStatus(_dataSource.Id, PageSyncStatusType.ResyncArticle, PageSyncStatus.InProgress);

        // get all posts
        var client = await _wordpressManagerBase.InitClient(_dataSource);
        var allPosts = await _wordpressManagerBase.GetAllPosts(_dataSource, client);

        // sync articles from wp
        foreach (var post in allPosts)
        {
            using var auditingScope = _auditingManager.BeginScope();

            try
            {
                var article = await _articleAladinRepository.FirstOrDefaultAsync(x => x.ExternalId == post.Id.To<int>())
                              ?? await _articleAladinRepository.FirstOrDefaultAsync(x => x.Title.Equals(post.Title.Rendered));
                if (article is not null)
                {
                    var mediaIds = article.Medias?.Select(x => x.MediaId).ToList();
                    var medias = await _mediaAladinRepository.GetListAsync(_ => mediaIds.Contains(_.Id));

                    foreach (var media in medias)
                    {
                        await _wordpressManagerBase.PostMediaAsync(_dataSource, media);
                    }

                    await _mediaAladinRepository.UpdateManyAsync(medias);

                    await _wordpressManagerBase.UpdatePostDetails(_dataSource, post, article, medias, client);

                    article.LastSyncedAt = DateTime.UtcNow;
                    article.ExternalId ??= post.Id.To<int>();
                    await _articleAladinRepository.UpdateAsync(article, true);
                    await CheckFormatEntity(article, "resync");
                }
            }
            catch (Exception ex)
            {
                //Add exceptions
                _wordpressManagerBase.LogException(_auditingManager.Current.Log, ex, null, PageDataSourceConsts.AladinUrl, "DoReSyncPostAsync");
            }
            finally
            {
                //Always save the log
                await auditingScope.SaveAsync();
            }
        }

        // update re-sync status
        await _dataSourceManager.DoUpdateSyncStatus(_dataSource.Id, PageSyncStatusType.ResyncArticle, PageSyncStatus.Completed);
    }

    public async Task DoSyncCategoriesAsync()
    {
        _dataSource = await _dataSourceRepository.GetAsync(x => x.Url.Contains(PageDataSourceConsts.AladinUrl));
        if (_dataSource == null)
        {
            return;
        }

        var categories = (await _categoryAladinRepository.GetListAsync(x => x.CategoryType == CategoryType.Article))
            .Select(x => x.Name).Distinct().ToList();
        // Category
        await _wordpressManagerBase.DoSyncCategoriesAsync(_dataSource, categories);
    }

    public async Task UpdateExternalIdAsync()
    {
        // get data source
        _dataSource = await _dataSourceRepository.GetAsync(x => x.Url.Contains(PageDataSourceConsts.AladinUrl));
        if (_dataSource == null || !_dataSource.ShouldReSyncArticle)
        {
            return;
        }

        // get all posts
        var client = await _wordpressManagerBase.InitClient(_dataSource);
        var allPosts = await _wordpressManagerBase.GetAllPosts(_dataSource, client);

        var index = 1;
        var total = allPosts.Count();

        foreach (var post in allPosts)
        {
            try
            {
                var article = await _articleAladinRepository.FirstOrDefaultAsync(_ => _.Title.Equals(HttpUtility.HtmlDecode(post.Title.Rendered)));
                if (article is not null)
                {
                    article.ExternalId = post.Id;
                    await _articleAladinRepository.UpdateAsync(article, true);
                }
                else
                {
                    Console.WriteLine($"Not found: {post.Title.Rendered}/{post.Link}");
                }

                Console.WriteLine($"Processing: {index}/{total}");
                index++;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine($"Fail: {post.Title.Rendered}/{post.Link}");
            }
        }
    }

    public async Task RemoveExternalIdAsync()
    {
        var articleIds = (await _articleAladinRepository.GetQueryableAsync())
            .Where(x => x.DataSourceId == _dataSource.Id && x.Content != null && x.LastSyncedAt != null)
            .Select(x => x.Id).ToList();

        var number = 1;
        var total = articleIds.Count();
        foreach (var articleId in articleIds)
        {
            try
            {
                var article = await _articleAladinRepository.GetAsync(articleId);
                if (article is not null)
                {
                    article.ExternalId = null;
                    article.LastSyncedAt = null;
                    await _articleAladinRepository.UpdateAsync(article, true);
                }

                Console.WriteLine($"Article -> {number}/{total}");
                number++;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }

    private async Task CheckFormatEntity(Article articleEntity, string type = "sync")
    {
        try
        {
            var checkArticle = await _articleAladinRepository.GetAsync(articleEntity.Id);
        }
        catch (Exception e)
        {
            var date = DateTime.UtcNow;
            var lines = new List<string>()
            {
                $"Exception: {e.Message}",
                $"Article Id: {articleEntity.Id}"
            };
            var logFileName = $"C:\\Work\\ErrorLogs\\Sites\\error-records_{type}_aladin_{date:dd-MM-yyyy_hh-mm}.txt";
            await File.WriteAllLinesAsync(logFileName, lines);
            throw;
        }
    }

    public async Task DoUpdatePostAsync()
    {
        _dataSource = await _dataSourceRepository.GetAsync(x => x.Url.Contains(PageDataSourceConsts.AladinUrl));
        if (_dataSource == null)
        {
            return;
        }

        var articleIds = (await _articleAladinRepository.GetQueryableAsync())
            .Where(x => x.DataSourceId == _dataSource.Id)
            .Select(x => x.Id).ToList();

        var client = new WordPressClient($"{_dataSource.PostToSite}/wp-json/");
        client.Auth.UseBasicAuth(_dataSource.Configuration.Username, _dataSource.Configuration.Password);

        var posts = new List<Post>();
        var pageIndex = 1;
        while (true)
        {
            //var route = "posts".SetQueryParam("status", "pending").SetQueryParam("per_page", "100").SetQueryParam("page", pageIndex.ToString());
            var resultPosts = await client.Posts.QueryAsync(new PostsQueryBuilder()
                {
                    Statuses = new List<Status>()
                    {
                        Status.Publish,
                        Status.Pending
                    },
                    Page = pageIndex,
                    PerPage = 100
                },
                true);

            posts.AddRange(resultPosts);
            Console.WriteLine($"Page {pageIndex}");

            if (resultPosts.IsNullOrEmpty() || resultPosts.Count() < 100)
            {
                break;
            }

            pageIndex++;
        }


        foreach (var post in posts)
        {
            using var auditingScope = _auditingManager.BeginScope();

            try
            {
                var article = await _articleAladinRepository.GetAsync(x => x.ExternalId != null && x.ExternalId.Equals(post.Id.ToString()));
                var articleNav = await _articleAladinRepository.GetWithNavigationPropertiesAsync(article.Id);

                await _wordpressManagerBase.DoUpdatePostAsync(_dataSource, articleNav, post, null);
            }
            catch (Exception ex)
            {
                //Add exceptions
                //_wordpressManagerBase.LogException(_auditingManager.Current.Log, ex, $"{article.Id}", PageDataSourceConsts.LongChauUrl);
            }
            finally
            {
                //Always save the log
                await auditingScope.SaveAsync();
            }
        }
    }
}