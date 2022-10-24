﻿using System;
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

public class WordpressManagerSucKhoeDoiSong : DomainService
{
    private readonly ICategorySucKhoeDoiSongRepository _categorySucKhoeDoiSongRepository;
    private readonly IArticleSucKhoeDoiSongRepository  _articleSucKhoeDoiSongRepository;
    private readonly IMediaSucKhoeDoiSongRepository    _mediaSucKhoeDoiSongRepository;
    private readonly IDataSourceRepository             _dataSourceRepository;
    private          DataSource                        _dataSource;
    private readonly WordpressManagerBase              _wordpressManagerBase;

    public WordpressManagerSucKhoeDoiSong(ICategorySucKhoeDoiSongRepository categorySucKhoeDoiSongRepository, 
                                          IArticleSucKhoeDoiSongRepository  articleSucKhoeDoiSongRepository, 
                                          IMediaSucKhoeDoiSongRepository    mediaSucKhoeDoiSongRepository,
                                          IDataSourceRepository             dataSourceRepository,
                                          WordpressManagerBase              wordpressManagerBase)
    {
        _categorySucKhoeDoiSongRepository = categorySucKhoeDoiSongRepository;
        _articleSucKhoeDoiSongRepository  = articleSucKhoeDoiSongRepository;
        _mediaSucKhoeDoiSongRepository    = mediaSucKhoeDoiSongRepository;
        _dataSourceRepository             = dataSourceRepository;
        _wordpressManagerBase             = wordpressManagerBase;
    }

    public async Task DoSyncPostAsync()
    {
        _dataSource = await _dataSourceRepository.FirstOrDefaultAsync(x => x.Url.Contains(PageDataSourceConsts.SucKhoeDoiSongUrl));
        if (_dataSource == null)
        {
            return;
        }
        
        var articleIds = (await _articleSucKhoeDoiSongRepository.GetQueryableAsync())
                        .Where(x => x.DataSourceId == _dataSource.Id && x.LastSyncedAt == null)
                        .Select(x=>x.Id);
        
        foreach (var articleId in articleIds)
        {
            var articleNav = await _articleSucKhoeDoiSongRepository.GetWithNavigationPropertiesAsync(articleId);
            var post       = await _wordpressManagerBase.DoSyncPostAsync(_dataSource, articleNav);
            if (post is not null) 
            {
                var article = await _articleSucKhoeDoiSongRepository.GetAsync(articleId);
                article.LastSyncedAt = DateTime.UtcNow;
                await _articleSucKhoeDoiSongRepository.UpdateAsync(article, true);

                if (articleNav.Media is not null) 
                {
                    await _mediaSucKhoeDoiSongRepository.UpdateAsync(articleNav.Media, true);
                }

                if (articleNav.Medias is not null)
                {
                    await _mediaSucKhoeDoiSongRepository.UpdateManyAsync(articleNav.Medias, true);
                }
            }
        }
    }

    public async Task DoSyncCategoriesAsync()
    {
        _dataSource = await _dataSourceRepository.GetAsync(x => x.Url.Contains(PageDataSourceConsts.SucKhoeDoiSongUrl));
        if (_dataSource == null)
        {
            return;
        }

        var categories = (await _categorySucKhoeDoiSongRepository.GetListAsync(x => x.CategoryType == CategoryType.Article))
                        .Select(x => x.Name).Distinct().ToList();
        // Category
        await _wordpressManagerBase.DoSyncCategoriesAsync(_dataSource, categories);
    }
}