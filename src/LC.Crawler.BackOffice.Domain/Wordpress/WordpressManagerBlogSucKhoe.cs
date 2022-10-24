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

public class WordpressManagerBlogSucKhoe : DomainService
{
    private readonly ICategoryBlogSucKhoeRepository _categoryBlogSucKhoeRepository;
    private readonly IArticleBlogSucKhoeRepository  _articleBlogSucKhoeRepository;
    private readonly IMediaBlogSucKhoeRepository    _mediaBlogSucKhoeRepository;
    private readonly IDataSourceRepository          _dataSourceRepository;
    private          DataSource                     _dataSource;
    private readonly WordpressManagerBase           _wordpressManagerBase;

    public WordpressManagerBlogSucKhoe(ICategoryBlogSucKhoeRepository categoryBlogSucKhoeRepository, 
                                       IArticleBlogSucKhoeRepository  articleBlogSucKhoeRepository, 
                                       IMediaBlogSucKhoeRepository    mediaBlogSucKhoeRepository, 
                                       IDataSourceRepository          dataSourceRepository,
                                       WordpressManagerBase           wordpressManagerBase)
    {
        _categoryBlogSucKhoeRepository = categoryBlogSucKhoeRepository;
        _articleBlogSucKhoeRepository  = articleBlogSucKhoeRepository;
        _mediaBlogSucKhoeRepository    = mediaBlogSucKhoeRepository;
        _dataSourceRepository          = dataSourceRepository;
        _wordpressManagerBase          = wordpressManagerBase;
    }

    public async Task DoSyncPostAsync()
    {
        _dataSource = await _dataSourceRepository.FirstOrDefaultAsync(x => x.Url.Contains(PageDataSourceConsts.BlogSucKhoeUrl));
        if (_dataSource == null)
        {
            return;
        }
        
        var articleIds = (await _articleBlogSucKhoeRepository.GetQueryableAsync())
                        .Where(x => x.DataSourceId == _dataSource.Id && x.LastSyncedAt == null)
                        .Select(x=>x.Id);
        
        foreach (var articleId in articleIds)
        {
            var articleNav = await _articleBlogSucKhoeRepository.GetWithNavigationPropertiesAsync(articleId);
            var post       = await _wordpressManagerBase.DoSyncPostAsync(_dataSource, articleNav);
            if (post is not null) 
            {
                var article = await _articleBlogSucKhoeRepository.GetAsync(articleId);
                article.LastSyncedAt = DateTime.UtcNow;
                await _articleBlogSucKhoeRepository.UpdateAsync(article, true);

                if (articleNav.Media is not null) 
                {
                    await _mediaBlogSucKhoeRepository.UpdateAsync(articleNav.Media, true);
                }

                if (articleNav.Medias is not null)
                {
                    await _mediaBlogSucKhoeRepository.UpdateManyAsync(articleNav.Medias, true);
                }
            }
        }
    }
    
    public async Task DoSyncCategoriesAsync()
    {
        _dataSource = await _dataSourceRepository.GetAsync(x => x.Url.Contains(PageDataSourceConsts.BlogSucKhoeUrl));
        if (_dataSource == null)
        {
            return;
        }

        var categories = (await _categoryBlogSucKhoeRepository.GetListAsync(x => x.CategoryType == CategoryType.Article))
                        .Select(x => x.Name).Distinct().ToList();
        // Category
        await _wordpressManagerBase.DoSyncCategoriesAsync(_dataSource, categories);
    }
}