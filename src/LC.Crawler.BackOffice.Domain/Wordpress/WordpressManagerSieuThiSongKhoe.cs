using System;
using System.Linq;
using System.Threading.Tasks;
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
        _dataSource = await _dataSourceRepository.FirstOrDefaultAsync(x => x.Url.Contains(PageDataSourceConsts.SieuThiSongKhoeUrl));
        if (_dataSource == null || !_dataSource.ShouldSyncArticle)
        {
            return;
        }
        
        // update re-sync status
        await _dataSourceManager.DoUpdateSyncStatus(_dataSource.Id, PageSyncStatusType.SyncArticle, PageSyncStatus.InProgress);

        
        // get article ids
        var articleIds = (await _articleSieuThiSongKhoeRepository.GetQueryableAsync())
                        .Where(x => x.DataSourceId == _dataSource.Id && x.LastSyncedAt == null)
                        .Select(x=>x.Id).ToList();
        
        // get all tags
        var wpTags = await _wordpressManagerBase.GetAllTags(_dataSource);
        
        // sync articles to wp
        foreach (var articleId in articleIds)
        {
            using var auditingScope = _auditingManager.BeginScope();
            var       articleNav    = await _articleSieuThiSongKhoeRepository.GetWithNavigationPropertiesAsync(articleId);
            
            try
            {
                var post = await _wordpressManagerBase.DoSyncPostAsync(_dataSource, articleNav, wpTags);
                if (post is not null) 
                {
                    var article = await _articleSieuThiSongKhoeRepository.GetAsync(articleId);
                    article.ExternalId   = post.Id.To<int>();
                    article.LastSyncedAt = DateTime.UtcNow;
                    await _articleSieuThiSongKhoeRepository.UpdateAsync(article, true);

                    if (articleNav.Media is not null) 
                    {
                        await _mediaSieuThiSongKhoeRepository.UpdateAsync(articleNav.Media, true);
                    }

                    if (articleNav.Medias.IsNotNullOrEmpty())
                    {
                        await _mediaSieuThiSongKhoeRepository.UpdateManyAsync(articleNav.Medias, true);
                    }
                }
            }
            catch (Exception ex)
            {
                //Add exceptions
                _wordpressManagerBase.LogException(_auditingManager.Current.Log, ex, articleNav.Article, 
                                                   PageDataSourceConsts.SieuThiSongKhoeUrl, "DoSyncPostAsync");
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
                var article = await _articleSieuThiSongKhoeRepository.FirstOrDefaultAsync(x => x.ExternalId == post.Id.To<int>()) 
                           ?? await _articleSieuThiSongKhoeRepository.FirstOrDefaultAsync(x => x.Title.Equals(post.Title.Rendered));
                if (article is not null)
                {
                    var mediaIds = article.Medias?.Select(x => x.MediaId).ToList();
                    var medias   = await _mediaSieuThiSongKhoeRepository.GetListAsync(_ => mediaIds.Contains(_.Id));
                    
                    await _wordpressManagerBase.UpdatePostDetails(post, article, medias, client);

                    article.LastSyncedAt =   DateTime.UtcNow;
                    article.ExternalId   ??= post.Id.To<int>();
                    await _articleSieuThiSongKhoeRepository.UpdateAsync(article, true);
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
}