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
using WooCategory = WordPressPCL.Models.Category;

namespace LC.Crawler.BackOffice.Wordpress;

public class WordpressManagerSongKhoeMedplus : DomainService
{
    private readonly IArticleSongKhoeMedplusRepository  _articleSongKhoeMedplusRepository;
    private readonly ICategorySongKhoeMedplusRepository _categorySongKhoeMedplusRepository;
    private readonly IMediaSongKhoeMedplusRepository    _mediaSongKhoeMedplusRepository;
    private readonly IDataSourceRepository              _dataSourceRepository;
    private          DataSource                         _dataSource;
    private readonly WordpressManagerBase               _wordpressManagerBase;

    public WordpressManagerSongKhoeMedplus(IArticleSongKhoeMedplusRepository  articleSongKhoeMedplusRepository, 
                                           ICategorySongKhoeMedplusRepository categorySongKhoeMedplusRepository, 
                                           IMediaSongKhoeMedplusRepository    mediaSongKhoeMedplusRepository, 
                                           IDataSourceRepository              dataSourceRepository,
                                           WordpressManagerBase               wordpressManagerBase)
    {
        _articleSongKhoeMedplusRepository  = articleSongKhoeMedplusRepository;
        _categorySongKhoeMedplusRepository = categorySongKhoeMedplusRepository;
        _mediaSongKhoeMedplusRepository    = mediaSongKhoeMedplusRepository;
        _dataSourceRepository              = dataSourceRepository;
        _wordpressManagerBase              = wordpressManagerBase;
    }

    public async Task DoSyncPostAsync()
    {
        _dataSource = await _dataSourceRepository.FirstOrDefaultAsync(x => x.Url.Contains(PageDataSourceConsts.SongKhoeMedplusUrl));
        if (_dataSource == null)
        {
            return;
        }
        
        var articleIds = (await _articleSongKhoeMedplusRepository.GetQueryableAsync())
                        .Where(x => x.DataSourceId == _dataSource.Id && x.LastSyncedAt == null)
                        .Select(x=>x.Id);
        
        foreach (var articleId in articleIds)
        {
            var articleNav = await _articleSongKhoeMedplusRepository.GetWithNavigationPropertiesAsync(articleId);
            var post       = await _wordpressManagerBase.DoSyncPostAsync(_dataSource, articleNav);
            if (post is not null) 
            {
                articleNav.Article.LastSyncedAt = DateTime.UtcNow;
                await _articleSongKhoeMedplusRepository.UpdateAsync(articleNav.Article, true);
                
                if (articleNav.Media is not null) 
                {
                    await _mediaSongKhoeMedplusRepository.UpdateAsync(articleNav.Media, true);
                }

                if (articleNav.Medias is not null)
                {
                    await _mediaSongKhoeMedplusRepository.UpdateManyAsync(articleNav.Medias, true);
                }
            }
        }
    }
    
    public async Task DoSyncCategoriesAsync()
    {
        _dataSource = await _dataSourceRepository.GetAsync(x => x.Url.Contains(PageDataSourceConsts.SongKhoeMedplusUrl));
        if (_dataSource == null)
        {
            return;
        }

        var categories = (await _categorySongKhoeMedplusRepository.GetListAsync(x => x.CategoryType == CategoryType.Article))
                        .Select(x => x.Name).Distinct().ToList();
        // Category
        await _wordpressManagerBase.DoSyncCategoriesAsync(_dataSource, categories);
    }
}