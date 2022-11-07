﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LC.Crawler.BackOffice.Categories;
using LC.Crawler.BackOffice.Core;
using LC.Crawler.BackOffice.DataSources;
using LC.Crawler.BackOffice.Enums;
using LC.Crawler.BackOffice.Extensions;
using LC.Crawler.BackOffice.Helpers;
using LC.Crawler.BackOffice.Medias;
using LC.Crawler.BackOffice.Payloads;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace LC.Crawler.BackOffice.Articles;

public class ArticleManangerAladin : DomainService
{
    private readonly IArticleAladinRepository _articleAladinRepository;
    private readonly ICategoryAladinRepository _categoryAladinRepository;
    private readonly IMediaAladinRepository _mediaAladinRepository;
    private readonly IDataSourceRepository _dataSourceRepository;
    
    public ArticleManangerAladin(IArticleAladinRepository articleAladinRepository, ICategoryAladinRepository categoryAladinRepository, IMediaAladinRepository mediaAladinRepository, IDataSourceRepository dataSourceRepository)
    {
        _articleAladinRepository = articleAladinRepository;
        _categoryAladinRepository = categoryAladinRepository;
        _mediaAladinRepository = mediaAladinRepository;
        _dataSourceRepository = dataSourceRepository;
    }

    public async Task ProcessingDataAsync(List<ArticlePayload> articles)
    {
       var dataSource = await _dataSourceRepository.GetAsync(x => x.Url.Contains(PageDataSourceConsts.AladinUrl));
        if (dataSource == null)
        {
            return;
        }

        var categories = await _categoryAladinRepository.GetListAsync(x=>x.CategoryType == CategoryType.Article);
        
        foreach (var rawArticles in articles.GroupBy(_ => _.Url))
        {
            var article = rawArticles.First();
                
            var articleEntity = await _articleAladinRepository.FirstOrDefaultAsync(x => x.Title.Equals(article.Title));
            if (articleEntity == null)
            {
                articleEntity = new Article(GuidGenerator.Create())
                {
                    Title = article.Title,
                    CreatedAt = article.CreatedAt,
                    Excerpt = article.ShortDescription,
                    Content = article.Content,
                    DataSourceId = dataSource.Id,
                    Tags = article.Tags
                };

                foreach (var raw in rawArticles)
                {
                    var category = categories.FirstOrDefault(x => x.Name == raw.Category);
                    if (category == null)
                    {
                        category = new Category()
                        {
                            Name = raw.Category,
                            CategoryType = CategoryType.Article
                        };
                        await _categoryAladinRepository.InsertAsync(category, true);
                        categories.Add(category);
                    }
                    
                    articleEntity.AddCategory(category.Id);
                }
                
                if (article.FeatureImage.IsNotNullOrEmpty())
                {
                    var media = new Media()
                    {
                        Url = article.FeatureImage,
                        IsDowloaded = false
                    };
                    await _mediaAladinRepository.InsertAsync(media, true);
                    articleEntity.FeaturedMediaId = media.Id;
                }

                if (!string.IsNullOrEmpty(article.Content))
                {
                    var mediaUrls = article.Content.GetImageUrls();

                    if (mediaUrls.Any())
                    {
                        var medias = mediaUrls.Select(url => new Media()
                        {
                            Url = url.Contains("http")? url : $"{dataSource.Url}{url}",
                            IsDowloaded = false
                        }).ToList();
                        await _mediaAladinRepository.InsertManyAsync(medias);

                        articleEntity.Content = StringHtmlHelper.SetContentMediaIds(article.Content, medias);

                        foreach (var media in medias)
                        {
                            articleEntity.AddMedia(media.Id);
                        }
                    }
                }

                await _articleAladinRepository.InsertAsync(articleEntity);
            }
        }
    }
}