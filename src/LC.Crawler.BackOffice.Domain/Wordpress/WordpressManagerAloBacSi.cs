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
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using WooCategory = WordPressPCL.Models.Category;

namespace LC.Crawler.BackOffice.Wordpress;

public class WordpressManagerAloBacSi : DomainService
{
    private readonly ICategoryAloBacSiRepository _categoryAloBacSiRepository;
    private readonly IArticleAloBacSiRepository  _articleAloBacSiRepository;
    private readonly IMediaAloBacSiRepository    _mediaAloBacSiRepository;
    private readonly IDataSourceRepository       _dataSourceRepository;
    private          DataSource                  _dataSource;
    private readonly WordpressManagerBase        _wordpressManagerBase;
    private readonly IAuditingManager            _auditingManager;

    public WordpressManagerAloBacSi(ICategoryAloBacSiRepository categoryAloBacSiRepository, 
                                    IArticleAloBacSiRepository  articleAloBacSiRepository, 
                                    IMediaAloBacSiRepository    mediaAloBacSiRepository, 
                                    IDataSourceRepository       dataSourceRepository,
                                    WordpressManagerBase        wordpressManagerBase,
                                    IAuditingManager            auditingManager)
    {
        _categoryAloBacSiRepository = categoryAloBacSiRepository;
        _articleAloBacSiRepository  = articleAloBacSiRepository;
        _mediaAloBacSiRepository    = mediaAloBacSiRepository;
        _dataSourceRepository       = dataSourceRepository;
        _wordpressManagerBase       = wordpressManagerBase;
        _auditingManager            = auditingManager;
    }

    public async Task UpdateDataPostAsync()
    {
        _dataSource = await _dataSourceRepository.FirstOrDefaultAsync(x => x.Url.Contains(PageDataSourceConsts.AloBacSiUrl));
        if (_dataSource == null)
        {
            return;
        }

        await _wordpressManagerBase.DoUpdatePosts(_dataSource);
    }
    public async Task DoSyncPostAsync()
    {
        // get datasource
        _dataSource = await _dataSourceRepository.FirstOrDefaultAsync(x => x.Url.Contains(PageDataSourceConsts.AloBacSiUrl));
        if (_dataSource == null || !_dataSource.ShouldSyncArticle)
        {
            return;
        }
        
        // update re-sync status
        _dataSource.ArticleSyncStatus   = PageSyncStatus.InProgress;
        _dataSource.LastArticleSyncedAt = DateTime.UtcNow; 
        await _dataSourceRepository.UpdateAsync(_dataSource, true);
        
        // get article ids
        var articleIds = (await _articleAloBacSiRepository.GetQueryableAsync())
                        .Where(x => x.DataSourceId == _dataSource.Id && x.LastSyncedAt == null)
                        .Select(x=>x.Id).ToList();
        
        // get all tags
        var wpTags = await _wordpressManagerBase.GetAllTags(_dataSource);
        
        // sync articles to wp
        foreach (var articleId in articleIds)
        {
            using var auditingScope = _auditingManager.BeginScope();
            var       articleNav    = await _articleAloBacSiRepository.GetWithNavigationPropertiesAsync(articleId);
            
            try
            {
                var post       = await _wordpressManagerBase.DoSyncPostAsync(_dataSource, articleNav, wpTags);
                if (post is not null) 
                {
                    var article = await _articleAloBacSiRepository.GetAsync(articleId);
                    article.LastSyncedAt = DateTime.UtcNow;
                    await _articleAloBacSiRepository.UpdateAsync(article, true);
                
                    if (articleNav.Media is not null) 
                    {
                        await _mediaAloBacSiRepository.UpdateAsync(articleNav.Media, true);
                    }

                    if (articleNav.Medias.IsNotNullOrEmpty())
                    {
                        await _mediaAloBacSiRepository.UpdateManyAsync(articleNav.Medias, true);
                    }
                }
            }
            catch (Exception ex)
            {
                //Add exceptions
                _wordpressManagerBase.LogException(_auditingManager.Current.Log, ex, articleNav.Article, PageDataSourceConsts.AloBacSiUrl);
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

    public async Task DoSyncCategoriesAsync()
    {
        _dataSource = await _dataSourceRepository.GetAsync(x => x.Url.Contains(PageDataSourceConsts.AloBacSiUrl));
        if (_dataSource == null)
        {
            return;
        }

        var categories = (await _categoryAloBacSiRepository.GetListAsync(x => x.CategoryType == CategoryType.Article))
                        .Select(x => x.Name).Distinct().ToList();
        // Category
        await _wordpressManagerBase.DoSyncCategoriesAsync(_dataSource, categories);
    }

}