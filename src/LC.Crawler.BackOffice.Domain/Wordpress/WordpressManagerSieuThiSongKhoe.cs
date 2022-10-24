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

    public WordpressManagerSieuThiSongKhoe(IDataSourceRepository              dataSourceRepository, 
                                           IMediaSieuThiSongKhoeRepository    mediaSieuThiSongKhoeRepository, 
                                           IArticleSieuThiSongKhoeRepository  articleSieuThiSongKhoeRepository,
                                           WordpressManagerBase               wordpressManagerBase,
                                           ICategorySieuThiSongKhoeRepository categorySieuThiSongKhoeRepository,
                                           IAuditingManager                   auditingManager)
    {
        _dataSourceRepository              = dataSourceRepository;
        _mediaSieuThiSongKhoeRepository    = mediaSieuThiSongKhoeRepository;
        _articleSieuThiSongKhoeRepository  = articleSieuThiSongKhoeRepository;
        _wordpressManagerBase              = wordpressManagerBase;
        _categorySieuThiSongKhoeRepository = categorySieuThiSongKhoeRepository;
        _auditingManager                   = auditingManager;
    }

    public async Task DoSyncPostAsync()
    {
        _dataSource = await _dataSourceRepository.FirstOrDefaultAsync(x => x.Url.Contains(PageDataSourceConsts.SieuThiSongKhoeUrl));
        if (_dataSource == null)
        {
            return;
        }
        
        var articleIds = (await _articleSieuThiSongKhoeRepository.GetQueryableAsync())
                        .Where(x => x.DataSourceId == _dataSource.Id && x.LastSyncedAt == null)
                        .Select(x=>x.Id);
        
        foreach (var articleId in articleIds)
        {
            using var auditingScope = _auditingManager.BeginScope();
            var       articleNav    = await _articleSieuThiSongKhoeRepository.GetWithNavigationPropertiesAsync(articleId);
            
            try
            {
                var post = await _wordpressManagerBase.DoSyncPostAsync(_dataSource, articleNav);
                if (post is not null) 
                {
                    var article = await _articleSieuThiSongKhoeRepository.GetAsync(articleId);
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
                _wordpressManagerBase.LogException(_auditingManager.Current.Log, ex, articleNav, PageDataSourceConsts.SieuThiSongKhoeUrl);
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