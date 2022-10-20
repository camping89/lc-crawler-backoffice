using System;
using System.Linq;
using System.Threading.Tasks;
using LC.Crawler.BackOffice.Articles;
using LC.Crawler.BackOffice.Categories;
using LC.Crawler.BackOffice.DataSources;
using LC.Crawler.BackOffice.Enums;
using LC.Crawler.BackOffice.Medias;
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

    public WordpressManagerSieuThiSongKhoe(IDataSourceRepository              dataSourceRepository, 
                                           IMediaSieuThiSongKhoeRepository    mediaSieuThiSongKhoeRepository, 
                                           IArticleSieuThiSongKhoeRepository  articleSieuThiSongKhoeRepository,
                                           WordpressManagerBase               wordpressManagerBase,
                                           ICategorySieuThiSongKhoeRepository categorySieuThiSongKhoeRepository)
    {
        _dataSourceRepository              = dataSourceRepository;
        _mediaSieuThiSongKhoeRepository    = mediaSieuThiSongKhoeRepository;
        _articleSieuThiSongKhoeRepository  = articleSieuThiSongKhoeRepository;
        _wordpressManagerBase              = wordpressManagerBase;
        _categorySieuThiSongKhoeRepository = categorySieuThiSongKhoeRepository;
    }

    public async Task DoSyncPostAsync()
    {
        _dataSource = await _dataSourceRepository.FirstOrDefaultAsync(x => x.Url.Contains(PageDataSourceConsts.SieuThiSongKhoeUrl));
        if (_dataSource == null)
        {
            return;
        }
        
        var articleIds = (await _articleSieuThiSongKhoeRepository.GetQueryableAsync())
                        .Where(x => x.DataSourceId == _dataSource.Id && x.LastSyncedAt.HasValue == false)
                        .Select(x=>x.Id);
        
        foreach (var articleId in articleIds)
        {
            var articleNav = await _articleSieuThiSongKhoeRepository.GetWithNavigationPropertiesAsync(articleId);
            var post       = await _wordpressManagerBase.DoSyncPostAsync(_dataSource, articleNav);
            if (post is not null) 
            {
                articleNav.Article.LastSyncedAt = DateTime.UtcNow;
                await _articleSieuThiSongKhoeRepository.UpdateAsync(articleNav.Article, true);
                
                if (articleNav.Media is not null) 
                {
                    await _mediaSieuThiSongKhoeRepository.UpdateAsync(articleNav.Media, true);
                }

                if (articleNav.Medias is not null)
                {
                    await _mediaSieuThiSongKhoeRepository.UpdateManyAsync(articleNav.Medias, true);
                }
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