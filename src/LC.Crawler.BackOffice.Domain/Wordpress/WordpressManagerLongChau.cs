using System;
using System.Linq;
using System.Threading.Tasks;
using LC.Crawler.BackOffice.Articles;
using LC.Crawler.BackOffice.DataSources;
using LC.Crawler.BackOffice.Medias;
using Volo.Abp.Domain.Services;
using LC.Crawler.BackOffice.Categories;
using LC.Crawler.BackOffice.Enums;
using Volo.Abp.Auditing;
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
        _dataSource = await _dataSourceRepository.GetAsync(x => x.Url.Contains(PageDataSourceConsts.LongChauUrl));
        if (_dataSource == null)
        {
            return;
        }
        
        var articleIds = (await _articleLongChauRepository.GetQueryableAsync())
                        .Where(x => x.DataSourceId == _dataSource.Id && x.LastSyncedAt == null)
                        .Select(x=>x.Id);

        foreach (var articleId in articleIds)
        {
            using var auditingScope = _auditingManager.BeginScope();
            var       articleNav    = await _articleLongChauRepository.GetWithNavigationPropertiesAsync(articleId);
            
            try
            {
                var post = await _wordpressManagerBase.DoSyncPostAsync(_dataSource, articleNav);
                if (post is not null)
                {
                    var article = await _articleLongChauRepository.GetAsync(articleId);
                    article.LastSyncedAt = DateTime.UtcNow;
                    await _articleLongChauRepository.UpdateAsync(article, true);
                
                    if (articleNav.Media is not null) 
                    {
                        await _mediaLongChauRepository.UpdateAsync(articleNav.Media, true);
                    }

                    if (articleNav.Medias is not null)
                    {
                        await _mediaLongChauRepository.UpdateManyAsync(articleNav.Medias, true);
                    }
                }
            }
            catch (Exception ex)
            {
                //Add exceptions
                _wordpressManagerBase.LogException(_auditingManager.Current.Log, ex, articleNav, PageDataSourceConsts.LongChauUrl);
            }
            finally
            {
                //Always save the log
                await auditingScope.SaveAsync();
            }
        }
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
}