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
using Newtonsoft.Json;
using Volo.Abp.Auditing;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using WordPressPCL;
using WordPressPCL.Models;
using WordPressPCL.Utility;
using WooCategory = WordPressPCL.Models.Category;

namespace LC.Crawler.BackOffice.Wordpress;

public class WordpressManagerSucKhoeDoiSong : DomainService
{
    private readonly ICategorySucKhoeDoiSongRepository _categorySucKhoeDoiSongRepository;
    private readonly IArticleSucKhoeDoiSongRepository  _articleSucKhoeDoiSongRepository;
    private readonly IMediaSucKhoeDoiSongRepository    _mediaSucKhoeDoiSongRepository;
    private readonly IDataSourceRepository             _dataSourceRepository;
    private          DataSource                        _dataSource;
    private readonly WordpressManagerBase              _wordpressManagerBase;
    private readonly IAuditingManager                  _auditingManager;
    private readonly DataSourceManager _dataSourceManager;

    public WordpressManagerSucKhoeDoiSong(ICategorySucKhoeDoiSongRepository categorySucKhoeDoiSongRepository, 
                                          IArticleSucKhoeDoiSongRepository  articleSucKhoeDoiSongRepository, 
                                          IMediaSucKhoeDoiSongRepository    mediaSucKhoeDoiSongRepository,
                                          IDataSourceRepository             dataSourceRepository,
                                          WordpressManagerBase              wordpressManagerBase,
                                          IAuditingManager                  auditingManager,
                                          DataSourceManager dataSourceManager)
    {
        _categorySucKhoeDoiSongRepository = categorySucKhoeDoiSongRepository;
        _articleSucKhoeDoiSongRepository  = articleSucKhoeDoiSongRepository;
        _mediaSucKhoeDoiSongRepository    = mediaSucKhoeDoiSongRepository;
        _dataSourceRepository             = dataSourceRepository;
        _wordpressManagerBase             = wordpressManagerBase;
        _auditingManager                  = auditingManager;
        _dataSourceManager = dataSourceManager;
    }

    public async Task DoSyncPostAsync()
    {
        // get datasource
        _dataSource = await _dataSourceRepository.FirstOrDefaultAsync(x => x.Url.Contains(PageDataSourceConsts.SucKhoeDoiSongUrl));
        if (_dataSource is not { ShouldSyncArticle: true })
        {
            return;
        }
        
        // update re-sync status
        await _dataSourceManager.DoUpdateSyncStatus(_dataSource.Id, PageSyncStatusType.SyncArticle, PageSyncStatus.InProgress);
        
        // get article ids
        var limitDate = new DateTime(2018, 01, 01); 
        var articleIds = (await _articleSucKhoeDoiSongRepository.GetQueryableAsync())
                        .Where(x => x.DataSourceId == _dataSource.Id
                                    && x.Content      != null
                                    && x.LastSyncedAt == null
                                    && x.CreatedAt >= limitDate)
                        .Select(x=>x.Id).ToList();
        
        // get categories
        //TODO Remove after clean data
        var categories = (await _categorySucKhoeDoiSongRepository.GetListAsync()).Where(_ => 
            !_.Name.Contains("Thời Sự", StringComparison.InvariantCultureIgnoreCase)).ToList();
        
        var categoryIds = categories.Select(_ => _.Id).ToList();
        
        // get all tags
        var wpTags = await _wordpressManagerBase.GetAllTags(_dataSource);

        var index = 1;
        var total = articleIds.Count();
        
        Console.WriteLine($"Start sync");
        // sync article to wp
        foreach (var articleId in articleIds)
        {
            Console.WriteLine($"Processing {index}/{total}");
            
            using var auditingScope = _auditingManager.BeginScope();
            
            try
            {
                var articleNav = await _articleSucKhoeDoiSongRepository.GetWithNavigationPropertiesAsync(articleId);
                if (articleNav.Categories.Any(_ => !_.Id.IsIn(categoryIds))) continue;
                
                var featureMedia = await _wordpressManagerBase.PostMediaAsync(_dataSource, articleNav.Media);
                await _wordpressManagerBase.PostMediasAsync(_dataSource, articleNav);
                
                if (articleNav.Media is not null)
                {
                    await _mediaSucKhoeDoiSongRepository.UpdateAsync(articleNav.Media, true);
                }

                if (articleNav.Medias.IsNotNullOrEmpty())
                {
                    await _mediaSucKhoeDoiSongRepository.UpdateManyAsync(articleNav.Medias, true);
                }

                var post = await _wordpressManagerBase.DoSyncPostAsync(_dataSource, articleNav, wpTags, featureMedia);
                
                if (post is not null) 
                {
                    var article = await _articleSucKhoeDoiSongRepository.GetAsync(articleId);
                    article.ExternalId   = post.Id.To<int>();
                    article.LastSyncedAt = DateTime.UtcNow;
                    await _articleSucKhoeDoiSongRepository.UpdateAsync(article, true);
                    await CheckFormatEntity(article);
                }
            }
            catch (Exception ex)
            {
                //Add exceptions
                _wordpressManagerBase.LogException(_auditingManager.Current.Log, ex, $"{articleId}", PageDataSourceConsts.SucKhoeDoiSongUrl, "DoSyncPostAsync");
            }
            finally
            {
                //Always save the log
                await auditingScope.SaveAsync();
            }

            index++;
        }
        
        // update re-sync status
        await _dataSourceManager.DoUpdateSyncStatus(_dataSource.Id, PageSyncStatusType.SyncArticle, PageSyncStatus.Completed);
        
        Console.WriteLine($"Finish sync");
    }
    
    public async Task DoReSyncPostAsync()
    {
        // get data source
        _dataSource = await _dataSourceRepository.GetAsync(x => x.Url.Contains(PageDataSourceConsts.SucKhoeDoiSongUrl));
        if (_dataSource == null || !_dataSource.ShouldReSyncArticle)
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
                var article = await _articleSucKhoeDoiSongRepository.FirstOrDefaultAsync(x => x.ExternalId == post.Id.To<int>()) 
                           ?? await _articleSucKhoeDoiSongRepository.FirstOrDefaultAsync(x => x.Title.Equals(post.Title.Rendered));
                if (article is not null)
                {
                    var mediaIds = article.Medias?.Select(x => x.MediaId).ToList();
                    var medias   = await _mediaSucKhoeDoiSongRepository.GetListAsync(_ => mediaIds.Contains(_.Id));
                    foreach (var media in medias)
                    {
                        await _wordpressManagerBase.PostMediaAsync(_dataSource, media);
                    }
                    await _mediaSucKhoeDoiSongRepository.UpdateManyAsync(medias);
                    
                    await _wordpressManagerBase.UpdatePostDetails(_dataSource,post, article, medias, client);

                    article.LastSyncedAt =   DateTime.UtcNow;
                    article.ExternalId   ??= post.Id.To<int>();
                    await _articleSucKhoeDoiSongRepository.UpdateAsync(article, true);
                    await CheckFormatEntity(article, "resync");
                }   
            }
            catch (Exception ex)
            {
                //Add exceptions
                _wordpressManagerBase.LogException(_auditingManager.Current.Log, ex, null, PageDataSourceConsts.SucKhoeDoiSongUrl, "DoReSyncPostAsync");
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
        _dataSource = await _dataSourceRepository.GetAsync(x => x.Url.Contains(PageDataSourceConsts.SucKhoeDoiSongUrl));
        if (_dataSource == null)
        {
            return;
        }
        
        var categories = (await _categorySucKhoeDoiSongRepository.GetListAsync(x => x.CategoryType == CategoryType.Article)).Where(x => !x.Name.Contains("Thời Sự", StringComparison.InvariantCultureIgnoreCase))
                .Select(x => x.Name).Distinct().ToList();
        // Category
        await _wordpressManagerBase.DoSyncCategoriesAsync(_dataSource, categories);
    }
    
    public async Task CheckContentFail() {
        // get article ids
        var articleIds = (await _articleSucKhoeDoiSongRepository.GetQueryableAsync())
                        .Where(x => x.Content != null)
                        .Select(x=> new
                         {
                             Id         = x.Id,
                             CreateTime = x.CreationTime
                         }).ToList().OrderByDescending(_ => _.CreateTime).Select(_ => _.Id).ToList();
        
        var index         = 1;
        var total         = articleIds.Count();
        
        // sync articles to wp
        foreach (var articleId in articleIds)
        {
            Console.WriteLine($"Processing {index}/{total}");
            
            try
            {
                var articleNav = await _articleSucKhoeDoiSongRepository.GetAsync(articleId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Remove ---------------- {articleId}");
                await _articleSucKhoeDoiSongRepository.DeleteOneById(articleId);
            }

            index++;
        }
    }
    
    public async Task DoUpdatePostAsync()
    {
        _dataSource = await _dataSourceRepository.GetAsync(x => x.Url.Contains(PageDataSourceConsts.SucKhoeDoiSongUrl));
        if (_dataSource == null)
        {
            return;
        }
        
        var articleIds = (await _articleSucKhoeDoiSongRepository.GetQueryableAsync())
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



        foreach (var articleId in articleIds)
        {
            using var auditingScope = _auditingManager.BeginScope();
            var       articleNav    = await _articleSucKhoeDoiSongRepository.GetWithNavigationPropertiesAsync(articleId);
           
            var wpPost = posts.FirstOrDefault(_ =>
                _.Title.Rendered.Equals(articleNav.Article.Title, StringComparison.InvariantCultureIgnoreCase));
            if(wpPost is null)
                continue;
            
            try
            {
                await _wordpressManagerBase.DoUpdatePostAsync(_dataSource, articleNav, wpPost, null);
            }
            catch (Exception ex)
            {
                //Add exceptions
                _wordpressManagerBase.LogException(_auditingManager.Current.Log, ex, $"{articleId}", PageDataSourceConsts.LongChauUrl);
            }
            finally
            {
                //Always save the log
                await auditingScope.SaveAsync();
            }
        }
    }
    
    private async Task CheckFormatEntity(Article articleEntity, string type = "sync")
    {
        try
        {
            var checkArticle = await _articleSucKhoeDoiSongRepository.GetAsync(articleEntity.Id);
        }
        catch (Exception e)
        {
            var date = DateTime.UtcNow;
            var lines = new List<string>()
            {
                $"Exception: {e.Message}",
                $"Article Id: {articleEntity.Id}"
            };
            var logFileName = $"C:\\Work\\ErrorLogs\\Sites\\rror-records_{type}_suckhoedoisong_{date:dd-MM-yyyy_hh-mm}.txt";
            await File.WriteAllLinesAsync(logFileName, lines);
            throw;
        }
    }
}