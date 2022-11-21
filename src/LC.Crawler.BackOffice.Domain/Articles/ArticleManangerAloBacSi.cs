﻿using System.Collections.Generic;
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

public class ArticleManangerAloBacSi : DomainService
{
    private readonly IArticleAloBacSiRepository _articleAloBacSiRepository;
    private readonly ICategoryAloBacSiRepository _categoryAloBacSiRepository;
    private readonly IMediaAloBacSiRepository _mediaAloBacSiRepository;
    private readonly IDataSourceRepository _dataSourceRepository;
    
    public ArticleManangerAloBacSi(IArticleAloBacSiRepository articleAloBacSiRepository, ICategoryAloBacSiRepository categoryAloBacSiRepository, IMediaAloBacSiRepository mediaAloBacSiRepository, IDataSourceRepository dataSourceRepository)
    {
        _articleAloBacSiRepository = articleAloBacSiRepository;
        _categoryAloBacSiRepository = categoryAloBacSiRepository;
        _mediaAloBacSiRepository = mediaAloBacSiRepository;
        _dataSourceRepository = dataSourceRepository;
    }

    public async Task ProcessingDataAsync(List<ArticlePayload> articles)
    {
       var dataSource = await _dataSourceRepository.GetAsync(x => x.Url.Contains(PageDataSourceConsts.AloBacSiUrl));
        if (dataSource == null)
        {
            return;
        }

        var categories = await _categoryAloBacSiRepository.GetListAsync(x=>x.CategoryType == CategoryType.Article);
        
        foreach (var rawArticles in articles.GroupBy(_ => _.Url))
        {
            var article = rawArticles.First();
            if (article.Content is null)
            {
                continue;
            }
            
            var articleEntity = await _articleAloBacSiRepository.FirstOrDefaultAsync(x => x.Title.Equals(article.Title));
            if (articleEntity == null)
            {
                articleEntity = new Article(GuidGenerator.Create())
                {
                    Title        = article.Title,
                    CreatedAt    = article.CreatedAt,
                    Excerpt      = article.ShortDescription,
                    Content      = article.Content,
                    DataSourceId = dataSource.Id,
                    Tags         = article.Tags,
                    Url          = article.Url
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
                        await _categoryAloBacSiRepository.InsertAsync(category, true);
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
                    await _mediaAloBacSiRepository.InsertAsync(media, true);
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
                        await _mediaAloBacSiRepository.InsertManyAsync(medias);

                        articleEntity.Content = StringHtmlHelper.SetContentMediaIds(article.Content, medias);

                        foreach (var media in medias)
                        {
                            articleEntity.AddMedia(media.Id);
                        }
                    }
                }

                await _articleAloBacSiRepository.InsertAsync(articleEntity);
            }
            else
            {
                if (string.IsNullOrEmpty(articleEntity.Url))
                {
                    articleEntity.Url = article.Url;
                    await _articleAloBacSiRepository.UpdateAsync(articleEntity);
                }
            }
        }
    }
    
    public async Task<List<KeyValuePair<string, int>>> CountArticleByCategory()
    {
        var articles = await _articleAloBacSiRepository.GetListAsync();
        var categories = await _categoryAloBacSiRepository.GetListAsync(_ => _.CategoryType == CategoryType.Article);
        return categories.Select(category => new KeyValuePair<string, int>(category.Name, articles.Count(_ => _.Categories.Select(c => c.CategoryId).Contains(category.Id)))).ToList();
    }
}