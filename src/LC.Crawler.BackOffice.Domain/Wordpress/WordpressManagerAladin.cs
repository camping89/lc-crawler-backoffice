using System;
using System.Linq;
using System.Threading.Tasks;
using LC.Crawler.BackOffice.Articles;
using LC.Crawler.BackOffice.DataSources;
using LC.Crawler.BackOffice.Medias;
using Volo.Abp.Domain.Services;
using LC.Crawler.BackOffice.Categories;
using LC.Crawler.BackOffice.Enums;
using LC.Crawler.BackOffice.Extensions;
using Volo.Abp.Auditing;
using WordpresCategory = WordPressPCL.Models.Category;

namespace LC.Crawler.BackOffice.Wordpress;

public class WordpressManagerAladin : DomainService
{
    private readonly IArticleAladinRepository  _articleAladinRepository;
    private readonly IMediaAladinRepository    _mediaAladinRepository;
    private readonly ICategoryAladinRepository _categoryAladinRepository;
    private readonly IDataSourceRepository       _dataSourceRepository;
    private          DataSource                  _dataSource;
    private readonly WordpressManagerBase        _wordpressManagerBase;
    private readonly IAuditingManager            _auditingManager;
    
    public WordpressManagerAladin(IArticleAladinRepository  articleAladinRepository, 
                                    IMediaAladinRepository    mediaAladinRepository, 
                                    IDataSourceRepository       dataSourceRepository, 
                                    ICategoryAladinRepository categoryAladinRepository,
                                    WordpressManagerBase        wordpressManagerBase,
                                    IAuditingManager            auditingManager)
    {
        _articleAladinRepository  = articleAladinRepository;
        _mediaAladinRepository    = mediaAladinRepository;
        _dataSourceRepository       = dataSourceRepository;
        _categoryAladinRepository = categoryAladinRepository;
        _wordpressManagerBase       = wordpressManagerBase;
        _auditingManager            = auditingManager;
    }

    public async Task DoSyncPostAsync()
    {
        _dataSource = await _dataSourceRepository.GetAsync(x => x.Url.Contains(PageDataSourceConsts.AladinUrl));
        if (_dataSource == null)
        {
            return;
        }
        
        var articleIds = (await _articleAladinRepository.GetQueryableAsync())
                        .Where(x => x.DataSourceId == _dataSource.Id && x.LastSyncedAt == null)
                        .Select(x=>x.Id).ToList();

        foreach (var articleId in articleIds)
        {
            using var auditingScope = _auditingManager.BeginScope();
            var       articleNav    = await _articleAladinRepository.GetWithNavigationPropertiesAsync(articleId);
            
            try
            {
                var post = await _wordpressManagerBase.DoSyncPostAsync(_dataSource, articleNav);
                if (post is not null)
                {
                    var article = await _articleAladinRepository.GetAsync(articleId);
                    article.LastSyncedAt = DateTime.UtcNow;
                    await _articleAladinRepository.UpdateAsync(article, true);
                
                    if (articleNav.Media is not null) 
                    {
                        await _mediaAladinRepository.UpdateAsync(articleNav.Media, true);
                    }

                    if (articleNav.Medias.IsNotNullOrEmpty())
                    {
                        await _mediaAladinRepository.UpdateManyAsync(articleNav.Medias, true);
                    }
                }
            }
            catch (Exception ex)
            {
                //Add exceptions
                _wordpressManagerBase.LogException(_auditingManager.Current.Log, ex, articleNav.Article, PageDataSourceConsts.AladinUrl);
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
        _dataSource = await _dataSourceRepository.GetAsync(x => x.Url.Contains(PageDataSourceConsts.AladinUrl));
        if (_dataSource == null)
        {
            return;
        }

        var categories = (await _categoryAladinRepository.GetListAsync(x => x.CategoryType == CategoryType.Article))
                        .Select(x => x.Name).Distinct().ToList();
        // Category
        await _wordpressManagerBase.DoSyncCategoriesAsync(_dataSource, categories);
    }
}