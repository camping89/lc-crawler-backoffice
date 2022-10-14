using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LC.Crawler.BackOffice.Categories;
using LC.Crawler.BackOffice.Core;
using LC.Crawler.BackOffice.Extensions;
using LC.Crawler.BackOffice.Helpers;
using LC.Crawler.BackOffice.Medias;
using LC.Crawler.BackOffice.Payloads;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace LC.Crawler.BackOffice.Articles;

public class ArticleManangerLongChau : DomainService
{
    private readonly IArticleLongChauRepository _articleLongChauRepository;
    private readonly ICategoryLongChauRepository _categoryLongChauRepository;
    private readonly IMediaLongChauRepository _mediaLongChauRepository;
    
    public ArticleManangerLongChau(IArticleLongChauRepository articleLongChauRepository, ICategoryLongChauRepository categoryLongChauRepository, IMediaLongChauRepository mediaLongChauRepository)
    {
        _articleLongChauRepository = articleLongChauRepository;
        _categoryLongChauRepository = categoryLongChauRepository;
        _mediaLongChauRepository = mediaLongChauRepository;
    }

    public async Task ProcessingDataAsync(List<ArticlePayload> articles)
    {
        var categories = await _categoryLongChauRepository.GetListAsync();
        
        foreach (var article in articles)
        {
            var articleEntity = await _articleLongChauRepository.FirstOrDefaultAsync(x => x.Title.Equals(article.Title));
            if (articleEntity != null)
            {
                continue;
                // articleEntity.Title = article.Title;
                // articleEntity.CreatedAt = article.CreatedAt;
                // articleEntity.Excerpt = article.ShortDescription;
                // articleEntity.Content = article.Content;
                // articleEntity.Tags = article.Tags.JoinAsString(";");
                // articleEntity.ConcurrencyStamp = Guid.NewGuid().ToString("N");
                //
                // var category = categories.FirstOrDefault(x => x.Name == article.Category);
                // if (category == null)
                // {
                //     category = new Category()
                //     {
                //         Name = article.Category
                //     };
                //     await _categoryLongChauRepository.InsertAsync(category, true);
                //     categories.Add(category);
                // }
                // articleEntity.AddCategory(category.Id);
            }
            else
            {
                articleEntity = new Article(GuidGenerator.Create());
                var category = categories.FirstOrDefault(x => x.Name == article.Category);
                if (category == null)
                {
                    category = new Category()
                    {
                        Name = article.Category
                    };
                    await _categoryLongChauRepository.InsertAsync(category, true);
                    categories.Add(category);
                }
                
                articleEntity.AddCategory(category.Id);
                
                if (article.FeatureImage.IsNotNullOrEmpty())
                {
                    var media = new Media()
                    {
                        Url = article.FeatureImage,
                        IsDowloaded = false
                    };
                    await _mediaLongChauRepository.InsertAsync(media, true);
                    articleEntity.FeaturedMediaId = media.Id;
                }
                
                articleEntity.AddCategory(category.Id); 
                
                if (!string.IsNullOrEmpty(article.Content))
                {
                    var mediaUrls = article.Content.GetImageUrls();

                    if (mediaUrls.Any())
                    {
                        var medias = mediaUrls.Select(url => new Media()
                        {
                            Url = url,
                            IsDowloaded = false
                        }).ToList();
                        await _mediaLongChauRepository.InsertManyAsync(medias);

                        articleEntity.Content = StringHtmlHelper.ReplaceImageUrls(article.Content, medias);
                        
                        foreach (var media in medias)
                        {
                            articleEntity.AddMedia(media.Id);
                        }
                    }
                }

                await _articleLongChauRepository.InsertAsync(articleEntity);
            }
        }
    }
    
}