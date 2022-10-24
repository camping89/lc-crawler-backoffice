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

public class WordpressManagerSongKhoeMedplus : DomainService
{
    private readonly IArticleSongKhoeMedplusRepository  _articleSongKhoeMedplusRepository;
    private readonly ICategorySongKhoeMedplusRepository _categorySongKhoeMedplusRepository;
    private readonly IMediaSongKhoeMedplusRepository    _mediaSongKhoeMedplusRepository;
    private readonly IDataSourceRepository              _dataSourceRepository;
    private          DataSource                         _dataSource;
    private readonly WordpressManagerBase               _wordpressManagerBase;
    private readonly IAuditingManager                   _auditingManager;

    public WordpressManagerSongKhoeMedplus(IArticleSongKhoeMedplusRepository  articleSongKhoeMedplusRepository, 
                                           ICategorySongKhoeMedplusRepository categorySongKhoeMedplusRepository, 
                                           IMediaSongKhoeMedplusRepository    mediaSongKhoeMedplusRepository, 
                                           IDataSourceRepository              dataSourceRepository,
                                           WordpressManagerBase               wordpressManagerBase,
                                           IAuditingManager                   auditingManager)
    {
        _articleSongKhoeMedplusRepository  = articleSongKhoeMedplusRepository;
        _categorySongKhoeMedplusRepository = categorySongKhoeMedplusRepository;
        _mediaSongKhoeMedplusRepository    = mediaSongKhoeMedplusRepository;
        _dataSourceRepository              = dataSourceRepository;
        _wordpressManagerBase              = wordpressManagerBase;
        _auditingManager                   = auditingManager;
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
            using var auditingScope = _auditingManager.BeginScope();
            var       articleNav    = await _articleSongKhoeMedplusRepository.GetWithNavigationPropertiesAsync(articleId);
            
            try
            {
                var post = await _wordpressManagerBase.DoSyncPostAsync(_dataSource, articleNav);
                if (post is not null) 
                {
                    var article = await _articleSongKhoeMedplusRepository.GetAsync(articleId);
                    article.LastSyncedAt = DateTime.UtcNow;
                    await _articleSongKhoeMedplusRepository.UpdateAsync(article, true);
                
                    if (articleNav.Media is not null) 
                    {
                        await _mediaSongKhoeMedplusRepository.UpdateAsync(articleNav.Media, true);
                    }

                    if (articleNav.Medias.IsNotNullOrEmpty())
                    {
                        await _mediaSongKhoeMedplusRepository.UpdateManyAsync(articleNav.Medias, true);
                    }
                }
            }
            catch (Exception ex)
            {
                //Add exceptions
                _wordpressManagerBase.LogException(_auditingManager.Current.Log, ex, articleNav, PageDataSourceConsts.SongKhoeMedplusUrl);
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