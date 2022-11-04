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

public class WordpressManagerSucKhoeGiaDinh : DomainService
{
    private readonly ICategorySucKhoeGiaDinhRepository _categorySucKhoeGiaDinhRepository;
    private readonly IArticleSucKhoeGiaDinhRepository  _articleSucKhoeGiaDinhRepository;
    private readonly IMediaSucKhoeGiaDinhRepository    _mediaSucKhoeGiaDinhRepository;
    private readonly IDataSourceRepository             _dataSourceRepository;
    private          DataSource                        _dataSource;
    private readonly WordpressManagerBase              _wordpressManagerBase;
    private readonly IAuditingManager                  _auditingManager;

    public WordpressManagerSucKhoeGiaDinh(ICategorySucKhoeGiaDinhRepository categorySucKhoeGiaDinhRepository, 
                                          IArticleSucKhoeGiaDinhRepository  articleSucKhoeGiaDinhRepository, 
                                          IMediaSucKhoeGiaDinhRepository    mediaSucKhoeGiaDinhRepository, 
                                          IDataSourceRepository             dataSourceRepository,
                                          WordpressManagerBase              wordpressManagerBase,
                                          IAuditingManager                  auditingManager)
    {
        _categorySucKhoeGiaDinhRepository = categorySucKhoeGiaDinhRepository;
        _articleSucKhoeGiaDinhRepository  = articleSucKhoeGiaDinhRepository;
        _mediaSucKhoeGiaDinhRepository    = mediaSucKhoeGiaDinhRepository;
        _dataSourceRepository             = dataSourceRepository;
        _wordpressManagerBase             = wordpressManagerBase;
        _auditingManager                  = auditingManager;
    }

    public async Task DoSyncPostAsync()
    {
        _dataSource = await _dataSourceRepository.FirstOrDefaultAsync(x => x.Url.Contains(PageDataSourceConsts.SucKhoeGiaDinhUrl));
        if (_dataSource == null)
        {
            return;
        }
        
        var articleIds = (await _articleSucKhoeGiaDinhRepository.GetQueryableAsync())
                        .Where(x => x.DataSourceId == _dataSource.Id && x.LastSyncedAt == null)
                        .Select(x=>x.Id).ToList();
        
        foreach (var articleId in articleIds)
        {
            using var auditingScope = _auditingManager.BeginScope();
            var       articleNav    = await _articleSucKhoeGiaDinhRepository.GetWithNavigationPropertiesAsync(articleId);
            
            try
            {
                var post       = await _wordpressManagerBase.DoSyncPostAsync(_dataSource, articleNav);
                if (post is not null) 
                {
                    var article = await _articleSucKhoeGiaDinhRepository.GetAsync(articleId);
                    article.LastSyncedAt = DateTime.UtcNow;
                    await _articleSucKhoeGiaDinhRepository.UpdateAsync(article, true);

                    if (articleNav.Media is not null) 
                    {
                        await _mediaSucKhoeGiaDinhRepository.UpdateAsync(articleNav.Media, true);
                    }

                    if (articleNav.Medias.IsNotNullOrEmpty())
                    {
                        await _mediaSucKhoeGiaDinhRepository.UpdateManyAsync(articleNav.Medias, true);
                    }
                }
            }
            catch (Exception ex)
            {
                //Add exceptions
                _wordpressManagerBase.LogException(_auditingManager.Current.Log, ex, articleNav.Article, PageDataSourceConsts.SucKhoeGiaDinhUrl);
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
        _dataSource = await _dataSourceRepository.GetAsync(x => x.Url.Contains(PageDataSourceConsts.SucKhoeGiaDinhUrl));
        if (_dataSource == null)
        {
            return;
        }

        var categories = (await _categorySucKhoeGiaDinhRepository.GetListAsync(x => x.CategoryType == CategoryType.Article))
                        .Select(x => x.Name).Distinct().ToList();
        // Category
        await _wordpressManagerBase.DoSyncCategoriesAsync(_dataSource, categories);
    }
}