using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Extensions;
using LC.Crawler.BackOffice.Articles;
using LC.Crawler.BackOffice.DataSources;
using LC.Crawler.BackOffice.Medias;
using Volo.Abp.Domain.Services;
using LC.Crawler.BackOffice.Categories;
using LC.Crawler.BackOffice.Enums;
using LC.Crawler.BackOffice.Extensions;
using Newtonsoft.Json;
using Volo.Abp.Auditing;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using WordPressPCL;
using WordPressPCL.Models;
using WordPressPCL.Utility;
using Guid = System.Guid;
using WordpresCategory = WordPressPCL.Models.Category;

namespace LC.Crawler.BackOffice.Wordpress;

public class WordpressManagerLongChau : DomainService
{
    private readonly IArticleLongChauRepository _articleLongChauRepository;
    private readonly IMediaLongChauRepository _mediaLongChauRepository;
    private readonly ICategoryLongChauRepository _categoryLongChauRepository;
    private readonly IDataSourceRepository _dataSourceRepository;
    private DataSource _dataSource;
    private readonly WordpressManagerBase _wordpressManagerBase;
    private readonly IAuditingManager _auditingManager;
    private readonly DataSourceManager _dataSourceManager;

    public WordpressManagerLongChau(IArticleLongChauRepository articleLongChauRepository,
        IMediaLongChauRepository mediaLongChauRepository,
        IDataSourceRepository dataSourceRepository,
        ICategoryLongChauRepository categoryLongChauRepository,
        WordpressManagerBase wordpressManagerBase,
        IAuditingManager auditingManager,
        DataSourceManager dataSourceManager)
    {
        _articleLongChauRepository = articleLongChauRepository;
        _mediaLongChauRepository = mediaLongChauRepository;
        _dataSourceRepository = dataSourceRepository;
        _categoryLongChauRepository = categoryLongChauRepository;
        _wordpressManagerBase = wordpressManagerBase;
        _auditingManager = auditingManager;
        _dataSourceManager = dataSourceManager;
    }

    public async Task DoSyncPostAsync()
    {
        // get datasource
        _dataSource = await _dataSourceRepository.GetAsync(x => x.Url.Contains(PageDataSourceConsts.LongChauUrl));
        // if (_dataSource is not { ShouldSyncArticle: true })
        // {
        //     return;
        // }

        // update re-sync status
        await _dataSourceManager.DoUpdateSyncStatus(_dataSource.Id, PageSyncStatusType.SyncArticle,
            PageSyncStatus.InProgress);

        // get article ids
        var limitDate = DateTime.UtcNow.AddDays(-45);
        var articleIds = (await _articleLongChauRepository.GetQueryableAsync())
            .Where(x => x.DataSourceId == _dataSource.Id && x.ExternalId == null  && x.Content != null).ToList().OrderByDescending(x => x.CreationTime).Take(200)
            .Select(x => x.Id).ToList();

        // get all tags
        var wpTags = await _wordpressManagerBase.GetAllTags(_dataSource);

        var count = 0;
        var total = articleIds.Count;
        // sync articles to wp
        foreach (var articleId in articleIds)
        {
            using var auditingScope = _auditingManager.BeginScope();

            try
            {
                count++;
                Console.WriteLine($"Progressing: {count}/{total}");
                var articleNav = await _articleLongChauRepository.GetWithNavigationPropertiesAsync(articleId);

                var featureMedia = await _wordpressManagerBase.PostMediaAsync(_dataSource, articleNav.Media);
                await _wordpressManagerBase.PostMediasAsync(_dataSource, articleNav);
                
                if (articleNav.Media is not null)
                {
                    await _mediaLongChauRepository.UpdateAsync(articleNav.Media, true);
                }

                if (articleNav.Medias.IsNotNullOrEmpty())
                {
                    await _mediaLongChauRepository.UpdateManyAsync(articleNav.Medias, true);
                }

                var post = await _wordpressManagerBase.DoSyncPostAsync(_dataSource, articleNav, wpTags, featureMedia);
                if (post is not null)
                {
                    var article = await _articleLongChauRepository.GetAsync(articleId);
                    article.ExternalId = post.Id.To<int>();
                    article.LastSyncedAt = DateTime.UtcNow;
                    await _articleLongChauRepository.UpdateAsync(article, true);
                    await CheckFormatEntity(article);
                }
            }
            catch (Exception ex)
            {
                //Add exceptions
                _wordpressManagerBase.LogException(_auditingManager.Current.Log, ex,
                    $"{articleId}", PageDataSourceConsts.LongChauUrl, "DoSyncPostAsync");
                Console.WriteLine($"Error ArticleId: {articleId}");
            }
            finally
            {
                //Always save the log
                await auditingScope.SaveAsync();
            }
        }

        // update re-sync status
        await _dataSourceManager.DoUpdateSyncStatus(_dataSource.Id, PageSyncStatusType.SyncArticle,
            PageSyncStatus.Completed);
    }

    public async Task DoReSyncPostAsync()
    {
        // get data source
        _dataSource = await _dataSourceRepository.GetAsync(x => x.Url.Contains(PageDataSourceConsts.LongChauUrl));
        if (_dataSource is not { ShouldReSyncArticle: true })
        {
            return;
        }

        // update re-sync status
        await _dataSourceManager.DoUpdateSyncStatus(_dataSource.Id, PageSyncStatusType.ResyncArticle,
            PageSyncStatus.InProgress);

        // get all posts
        var client = await _wordpressManagerBase.InitClient(_dataSource);
        var allPosts = await _wordpressManagerBase.GetAllPosts(_dataSource, client);

        // sync articles from wp
        foreach (var post in allPosts)
        {
            using var auditingScope = _auditingManager.BeginScope();

            try
            {
                var article =
                    await _articleLongChauRepository.FirstOrDefaultAsync(x => x.ExternalId == post.Id.To<int>())
                    ?? await _articleLongChauRepository.FirstOrDefaultAsync(x => x.Title.Equals(post.Title.Rendered));
                if (article is not null)
                {
                    var mediaIds = article.Medias?.Select(x => x.MediaId).ToList();
                    var medias = await _mediaLongChauRepository.GetListAsync(_ => mediaIds.Contains(_.Id));
                    foreach (var media in medias)
                    {
                        await _wordpressManagerBase.PostMediaAsync(_dataSource, media);
                    }
                    await _mediaLongChauRepository.UpdateManyAsync(medias);
                    
                    await _wordpressManagerBase.UpdatePostDetails(_dataSource, post, article, medias, client);
                    
                    article.LastSyncedAt = DateTime.UtcNow;
                    article.ExternalId ??= post.Id.To<int>();
                    await _articleLongChauRepository.UpdateAsync(article, true);
                    await CheckFormatEntity(article, "resync");
                }
            }
            catch (Exception ex)
            {
                //Add exceptions
                _wordpressManagerBase.LogException(_auditingManager.Current.Log, ex, null,
                    PageDataSourceConsts.LongChauUrl, "DoReSyncPostAsync");
            }
            finally
            {
                //Always save the log
                await auditingScope.SaveAsync();
            }
        }

        // update re-sync status
        await _dataSourceManager.DoUpdateSyncStatus(_dataSource.Id, PageSyncStatusType.ResyncArticle,
            PageSyncStatus.Completed);
    }

    public async Task DoSyncCategoriesAsync()
    {
        _dataSource = await _dataSourceRepository.GetAsync(x => x.Url.Contains(PageDataSourceConsts.LongChauUrl));
        if (_dataSource == null)
        {
            return;
        }

        var categories = (await _categoryLongChauRepository.GetListAsync(x => x.CategoryType == CategoryType.Article))
            .Select(x => x.Name).Distinct().ToList();
        // Category
        await _wordpressManagerBase.DoSyncCategoriesAsync(_dataSource, categories);
    }

    public async Task DoUpdatePostAsync()
    {
        _dataSource = await _dataSourceRepository.GetAsync(x => x.Url.Contains(PageDataSourceConsts.LongChauUrl));
        if (_dataSource == null)
        {
            return;
        }

        var articleIds = (await _articleLongChauRepository.GetQueryableAsync())
            .Where(x => x.DataSourceId == _dataSource.Id)
            .Select(x => x.Id).ToList();

        var categories = await _categoryLongChauRepository.GetListAsync(_ => _.CategoryType == CategoryType.Article);

        var handleCategories = categories
            .Where(_ => _.Name.IsNotNullOrEmpty() && _.Name.Contains("->") && _.Name.Split("->").LastOrDefault().Contains('&')).ToList();

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
            },true);

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
                var article = await _articleLongChauRepository.GetAsync(x => x.ExternalId != null && x.ExternalId.Equals(post.Id.ToString()));
                var       articleNav    = await _articleLongChauRepository.GetWithNavigationPropertiesAsync(article.Id);

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

    public async Task CheckContentFail()
    {
        // get article ids
        var articleIds = (await _articleLongChauRepository.GetQueryableAsync())
            .Where(x => x.Content != null)
            .Select(x => new
            {
                Id = x.Id,
                CreateTime = x.CreationTime
            }).ToList().OrderByDescending(_ => _.CreateTime).Select(_ => _.Id).ToList();

        var index = 1;
        var total = articleIds.Count();
        var articleErrors = new List<Guid>();

        // sync articles to wp
        foreach (var articleId in articleIds)
        {
            Console.WriteLine($"Processing {index}/{total}");

            try
            {
                var articleNav = await _articleLongChauRepository.GetAsync(articleId);
            }
            catch (Exception ex)
            {
                articleErrors.Add(articleId);
                await _articleLongChauRepository.DeleteOneById(articleId);
                Console.WriteLine($"Remove ---------------- {articleId}");
            }

            index++;
        }
        Console.WriteLine($"TOTAL---------------- {articleErrors.Count()}");
    }
    
    private async Task CheckFormatEntity(Article articleEntity, string type = "sync")
    {
        try
        {
            var checkArticle = await _articleLongChauRepository.GetAsync(articleEntity.Id);
        }
        catch (Exception e)
        {
            var date = DateTime.UtcNow;
            var lines = new List<string>()
            {
                $"Exception: {e.Message}",
                $"Article Id: {articleEntity.Id}"
            };
            var logFileName = $"C:\\Work\\ErrorLogs\\Sites\\rror-records_{type}_longchau_{date:dd-MM-yyyy_hh-mm}.txt";
            await File.WriteAllLinesAsync(logFileName, lines);
            throw;
        }
    }
}