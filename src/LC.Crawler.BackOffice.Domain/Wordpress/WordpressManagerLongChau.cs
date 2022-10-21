using System;
using System.Linq;
using System.Threading.Tasks;
using LC.Crawler.BackOffice.Articles;
using LC.Crawler.BackOffice.DataSources;
using LC.Crawler.BackOffice.Medias;
using Volo.Abp.Domain.Services;
using LC.Crawler.BackOffice.Categories;
using LC.Crawler.BackOffice.Enums;
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
    
    public WordpressManagerLongChau(IArticleLongChauRepository  articleLongChauRepository, 
                                    IMediaLongChauRepository    mediaLongChauRepository, 
                                    IDataSourceRepository       dataSourceRepository, 
                                    ICategoryLongChauRepository categoryLongChauRepository,
                                    WordpressManagerBase        wordpressManagerBase)
    {
        _articleLongChauRepository  = articleLongChauRepository;
        _mediaLongChauRepository    = mediaLongChauRepository;
        _dataSourceRepository       = dataSourceRepository;
        _categoryLongChauRepository = categoryLongChauRepository;
        _wordpressManagerBase       = wordpressManagerBase;
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
            var articleNav = await _articleLongChauRepository.GetWithNavigationPropertiesAsync(articleId);
            var post       = await _wordpressManagerBase.DoSyncPostAsync(_dataSource, articleNav);
            if (post is not null) 
            {
                articleNav.Article.LastSyncedAt = DateTime.UtcNow;
                await _articleLongChauRepository.UpdateAsync(articleNav.Article, true);
                
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