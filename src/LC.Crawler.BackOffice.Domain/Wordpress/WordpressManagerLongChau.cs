using System;
using System.Collections.Generic;
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
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Repositories;
using WordPressPCL;
using WordPressPCL.Models;
using WordPressPCL.Utility;
using WordpresCategory = WordPressPCL.Models.Category;

namespace LC.Crawler.BackOffice.Wordpress;

public class WordpressManagerLongChau : DomainService
{
    private readonly IArticleLongChauRepository  _articleLongChauRepository;
    private readonly IMediaLongChauRepository    _mediaLongChauRepository;
    private readonly ICategoryLongChauRepository _categoryLongChauRepository;
    private readonly IDataSourceRepository       _dataSourceRepository;
    private          DataSource                  _dataSource;
    private readonly WordpressManagerBase        _wordpressManagerBase;
    private readonly IAuditingManager            _auditingManager;
    
    public WordpressManagerLongChau(IArticleLongChauRepository  articleLongChauRepository, 
                                    IMediaLongChauRepository    mediaLongChauRepository, 
                                    IDataSourceRepository       dataSourceRepository, 
                                    ICategoryLongChauRepository categoryLongChauRepository,
                                    WordpressManagerBase        wordpressManagerBase,
                                    IAuditingManager            auditingManager)
    {
        _articleLongChauRepository  = articleLongChauRepository;
        _mediaLongChauRepository    = mediaLongChauRepository;
        _dataSourceRepository       = dataSourceRepository;
        _categoryLongChauRepository = categoryLongChauRepository;
        _wordpressManagerBase       = wordpressManagerBase;
        _auditingManager            = auditingManager;
    }

    public async Task DoSyncPostAsync()
    {
        // get datasource
        _dataSource = await _dataSourceRepository.GetAsync(x => x.Url.Contains(PageDataSourceConsts.LongChauUrl));
        if (_dataSource == null || !_dataSource.ShouldSyncArticle)
        {
            return;
        }
        
        // update re-sync status
        _dataSource.ArticleSyncStatus   = PageSyncStatus.InProgress;
        _dataSource.LastArticleSyncedAt = DateTime.UtcNow; 
        await _dataSourceRepository.UpdateAsync(_dataSource, true);
        
        // get article ids
        var articleIds = (await _articleLongChauRepository.GetQueryableAsync())
                        .Where(x => x.DataSourceId == _dataSource.Id && x.LastSyncedAt == null)
                        .Select(x=>x.Id).ToList();

        // get all tags
        var wpTags = await _wordpressManagerBase.GetAllTags(_dataSource);
        
        // sync articles to wp
        foreach (var articleId in articleIds)
        {
            using var auditingScope = _auditingManager.BeginScope();
            var       articleNav    = await _articleLongChauRepository.GetWithNavigationPropertiesAsync(articleId);
            
            try
            {
                var post = await _wordpressManagerBase.DoSyncPostAsync(_dataSource, articleNav, wpTags);
                if (post is not null)
                {
                    var article = await _articleLongChauRepository.GetAsync(articleId);
                    article.ExternalId   = post.Id.To<int>();
                    article.LastSyncedAt = DateTime.UtcNow;
                    await _articleLongChauRepository.UpdateAsync(article, true);
                
                    if (articleNav.Media is not null) 
                    {
                        await _mediaLongChauRepository.UpdateAsync(articleNav.Media, true);
                    }

                    if (articleNav.Medias.IsNotNullOrEmpty())
                    {
                        await _mediaLongChauRepository.UpdateManyAsync(articleNav.Medias, true);
                    }
                }
            }
            catch (Exception ex)
            {
                //Add exceptions
                _wordpressManagerBase.LogException(_auditingManager.Current.Log, ex, 
                                                   articleNav.Article, PageDataSourceConsts.LongChauUrl, "DoSyncPostAsync");
            }
            finally
            {
                //Always save the log
                await auditingScope.SaveAsync();
            }
        }
        
        // update re-sync status
        _dataSource.ArticleSyncStatus   = PageSyncStatus.Completed;
        _dataSource.LastArticleSyncedAt = DateTime.UtcNow; 
        await _dataSourceRepository.UpdateAsync(_dataSource, true);
    }
    
    public async Task DoReSyncPostAsync()
    {
        // get data source
        _dataSource = await _dataSourceRepository.GetAsync(x => x.Url.Contains(PageDataSourceConsts.LongChauUrl));
        if (_dataSource == null || !_dataSource.ShouldReSyncArticle)
        {
            return;
        }
        
        // update re-sync status
        _dataSource.ArticleReSyncStatus   = PageSyncStatus.InProgress;
        _dataSource.LastArticleReSyncedAt = DateTime.UtcNow; 
        await _dataSourceRepository.UpdateAsync(_dataSource, true);
        
        // get all posts
        var client   = await _wordpressManagerBase.InitClient(_dataSource);
        var allPosts = await _wordpressManagerBase.GetAllPosts(_dataSource, client);

        // sync articles from wp
        foreach (var post in allPosts)
        {
            using var auditingScope = _auditingManager.BeginScope();
            
            try
            {
                var article = await _articleLongChauRepository.FirstOrDefaultAsync(x => x.ExternalId == post.Id.To<int>()) 
                           ?? await _articleLongChauRepository.FirstOrDefaultAsync(x => x.Title.Equals(post.Title.Rendered));
                if (article is not null)
                {
                    var mediaIds = article.Medias?.Select(x => x.MediaId).ToList();
                    var medias   = await _mediaLongChauRepository.GetListAsync(_ => mediaIds.Contains(_.Id));
                    
                    await _wordpressManagerBase.UpdatePostDetails(post, article, medias, client);

                    article.LastSyncedAt =   DateTime.UtcNow;
                    article.ExternalId   ??= post.Id.To<int>();
                    await _articleLongChauRepository.UpdateAsync(article, true);
                }   
            }
            catch (Exception ex)
            {
                //Add exceptions
                _wordpressManagerBase.LogException(_auditingManager.Current.Log, ex, null, PageDataSourceConsts.LongChauUrl, "DoReSyncPostAsync");
            }
            finally
            {
                //Always save the log
                await auditingScope.SaveAsync();
            }
        }
        
        // update re-sync status
        _dataSource.ArticleReSyncStatus   = PageSyncStatus.Completed;
        _dataSource.LastArticleReSyncedAt = DateTime.UtcNow; 
        await _dataSourceRepository.UpdateAsync(_dataSource, true);
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
            var       articleNav    = await _articleLongChauRepository.GetWithNavigationPropertiesAsync(articleId);
           
            var wpPost = posts.FirstOrDefault(_ =>
                _.Title.Rendered.Equals(articleNav.Article.Title, StringComparison.InvariantCultureIgnoreCase));
            if(wpPost is null)
                continue;
            
            try
            {
                await _wordpressManagerBase.DoUpdatePostAsync(_dataSource, articleNav, wpPost);
            }
            catch (Exception ex)
            {
                //Add exceptions
                _wordpressManagerBase.LogException(_auditingManager.Current.Log, ex, articleNav.Article, PageDataSourceConsts.LongChauUrl);
            }
            finally
            {
                //Always save the log
                await auditingScope.SaveAsync();
            }
        }
    }
}