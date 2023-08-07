using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Extensions;
using LC.Crawler.BackOffice.Articles;
using LC.Crawler.BackOffice.Categories;
using LC.Crawler.BackOffice.DataSources;
using LC.Crawler.BackOffice.Enums;
using LC.Crawler.BackOffice.Extensions;
using LC.Crawler.BackOffice.Medias;
using Volo.Abp.Auditing;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using WordPressPCL;
using WordPressPCL.Models;
using WordPressPCL.Utility;

namespace LC.Crawler.BackOffice.Wordpress;

public class WordpressManagerSieuThiSongKhoe : DomainService
{
    private readonly IArticleSieuThiSongKhoeRepository  _articleSieuThiSongKhoeRepository;
    private readonly IMediaSieuThiSongKhoeRepository    _mediaSieuThiSongKhoeRepository;
    private readonly IDataSourceRepository              _dataSourceRepository;
    private readonly ICategorySieuThiSongKhoeRepository _categorySieuThiSongKhoeRepository;
    private          DataSource                         _dataSource;
    private readonly WordpressManagerBase               _wordpressManagerBase;
    private readonly IAuditingManager                   _auditingManager;

    private readonly DataSourceManager _dataSourceManager;
    public WordpressManagerSieuThiSongKhoe(IDataSourceRepository              dataSourceRepository, 
                                           IMediaSieuThiSongKhoeRepository    mediaSieuThiSongKhoeRepository, 
                                           IArticleSieuThiSongKhoeRepository  articleSieuThiSongKhoeRepository,
                                           WordpressManagerBase               wordpressManagerBase,
                                           ICategorySieuThiSongKhoeRepository categorySieuThiSongKhoeRepository,
                                           IAuditingManager                   auditingManager,
                                           DataSourceManager dataSourceManager)
    {
        _dataSourceRepository              = dataSourceRepository;
        _mediaSieuThiSongKhoeRepository    = mediaSieuThiSongKhoeRepository;
        _articleSieuThiSongKhoeRepository  = articleSieuThiSongKhoeRepository;
        _wordpressManagerBase              = wordpressManagerBase;
        _categorySieuThiSongKhoeRepository = categorySieuThiSongKhoeRepository;
        _auditingManager                   = auditingManager;
        _dataSourceManager = dataSourceManager;
    }

    public async Task DoSyncPostAsync()
    {
        // get datasource
        Console.WriteLine($"Start Sync: {PageDataSourceConsts.SieuThiSongKhoeUrl}");
        _dataSource = await _dataSourceRepository.FirstOrDefaultAsync(x => x.Url.Contains(PageDataSourceConsts.SieuThiSongKhoeUrl));
        // if (_dataSource is not { ShouldReSyncArticle: true })
        // {
        //     return;
        // }
        
        // update re-sync status
        await _dataSourceManager.DoUpdateSyncStatus(_dataSource.Id, PageSyncStatusType.SyncArticle, PageSyncStatus.InProgress);

        
        // get article ids
        var limitDate = DateTime.UtcNow.AddDays(-45);
        var articleIds = (await _articleSieuThiSongKhoeRepository.GetQueryableAsync())
            .Where(x => x.DataSourceId == _dataSource.Id && x.Content != null).ToList().OrderByDescending(x => x.CreationTime).Take(200)
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
                var articleNav = await _articleSieuThiSongKhoeRepository.GetWithNavigationPropertiesAsync(articleId);
                
                var featureMedia = await _wordpressManagerBase.PostMediaAsync(_dataSource, articleNav.Media);
                await _wordpressManagerBase.PostMediasAsync(_dataSource, articleNav);
                
                if (articleNav.Media is not null)
                {
                    await _mediaSieuThiSongKhoeRepository.UpdateAsync(articleNav.Media, true);
                }

                if (articleNav.Medias.IsNotNullOrEmpty())
                {
                    await _mediaSieuThiSongKhoeRepository.UpdateManyAsync(articleNav.Medias, true);
                }

                var post = await _wordpressManagerBase.DoSyncPostAsync(_dataSource, articleNav, wpTags, featureMedia);
                if (post is not null) 
                {
                    var article = await _articleSieuThiSongKhoeRepository.GetAsync(articleId);
                    article.ExternalId   = post.Id.To<int>();
                    article.LastSyncedAt = DateTime.UtcNow;
                    await _articleSieuThiSongKhoeRepository.UpdateAsync(article, true);
                    await CheckFormatEntity(article);
                }
            }
            catch (Exception ex)
            {
                //Add exceptions
                _wordpressManagerBase.LogException(_auditingManager.Current.Log, ex, $"{articleId}", 
                                                   PageDataSourceConsts.SieuThiSongKhoeUrl, "DoSyncPostAsync");
                Console.WriteLine(articleId);
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
        _dataSource = await _dataSourceRepository.GetAsync(x => x.Url.Contains(PageDataSourceConsts.SieuThiSongKhoeUrl));
        if (_dataSource is not { ShouldReSyncArticle: true })
        {
            return;
        }
        
        // update re-sync status
        await _dataSourceManager.DoUpdateSyncStatus(_dataSource.Id, PageSyncStatusType.ResyncArticle, PageSyncStatus.InProgress);

        // get all posts
        var client   = await _wordpressManagerBase.InitClient(_dataSource);
        var allPosts = await _wordpressManagerBase.GetAllPosts(_dataSource, client);

        // sync articles from wp
        foreach (var post in allPosts)
        {
            using var auditingScope = _auditingManager.BeginScope();
            
            try
            {
                var article = await _articleSieuThiSongKhoeRepository.FirstOrDefaultAsync(x => x.ExternalId == post.Id.To<int>()) 
                           ?? await _articleSieuThiSongKhoeRepository.FirstOrDefaultAsync(x => x.Title.Equals(post.Title.Rendered));
                if (article is not null)
                {
                    var mediaIds = article.Medias?.Select(x => x.MediaId).ToList();
                    var medias   = await _mediaSieuThiSongKhoeRepository.GetListAsync(_ => mediaIds.Contains(_.Id));
                    
                    foreach (var media in medias)
                    {
                        await _wordpressManagerBase.PostMediaAsync(_dataSource, media);
                    }
                    await _mediaSieuThiSongKhoeRepository.UpdateManyAsync(medias);
                    
                    await _wordpressManagerBase.UpdatePostDetails(_dataSource ,post, article, medias, client);

                    article.LastSyncedAt =   DateTime.UtcNow;
                    article.ExternalId   ??= post.Id.To<int>();
                    await _articleSieuThiSongKhoeRepository.UpdateAsync(article, true);
                    await CheckFormatEntity(article, "resync");
                }   
            }
            catch (Exception ex)
            {
                //Add exceptions
                _wordpressManagerBase.LogException(_auditingManager.Current.Log, ex, null, PageDataSourceConsts.SieuThiSongKhoeUrl, "DoReSyncPostAsync");
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
    
    public async Task DoSyncCategoriesAsync()
    {
        _dataSource = await _dataSourceRepository.GetAsync(x => x.Url.Contains(PageDataSourceConsts.SieuThiSongKhoeUrl));
        if (_dataSource == null)
        {
            return;
        }

        var categories = (await _categorySieuThiSongKhoeRepository.GetListAsync(x => x.CategoryType == CategoryType.Article))
                        .Select(x => x.Name).Distinct().ToList();
        // Category
        await _wordpressManagerBase.DoSyncCategoriesAsync(_dataSource, categories);
    }
    
    private async Task CheckFormatEntity(Article articleEntity, string type = "sync")
    {
        try
        {
            var checkArticle = await _articleSieuThiSongKhoeRepository.GetAsync(articleEntity.Id);
        }
        catch (Exception e)
        {
            var date = DateTime.UtcNow;
            var lines = new List<string>()
            {
                $"Exception: {e.Message}",
                $"Article Id: {articleEntity.Id}"
            };
            var logFileName = $"C:\\Work\\ErrorLogs\\Sites\\error-records_{type}_sieuthisongkhoe_{date:dd-MM-yyyy_hh-mm}.txt";
            await File.WriteAllLinesAsync(logFileName, lines);
            throw;
        }
    }
    
    public async Task DoUpdatePostAsync()
    {
        _dataSource = await _dataSourceRepository.GetAsync(x => x.Url.Contains(PageDataSourceConsts.SieuThiSongKhoeUrl));
        if (_dataSource == null)
        {
            return;
        }
        
        var articleIds = (await _articleSieuThiSongKhoeRepository.GetQueryableAsync())
            .Where(x => x.DataSourceId == _dataSource.Id)
            .Select(x=>x.Id).ToList();
        
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
                var article = await _articleSieuThiSongKhoeRepository.GetAsync(x => x.ExternalId != null && x.ExternalId.Equals(post.Id.ToString()));
                var       articleNav    = await _articleSieuThiSongKhoeRepository.GetWithNavigationPropertiesAsync(article.Id);

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