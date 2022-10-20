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

public class ArticleManangerSucKhoeGiaDinh : DomainService
{
    private readonly IArticleSucKhoeGiaDinhRepository _articleSucKhoeGiaDinhRepository;
    private readonly ICategorySucKhoeGiaDinhRepository _categorySucKhoeGiaDinhRepository;
    private readonly IMediaSucKhoeGiaDinhRepository _mediaSucKhoeGiaDinhRepository;
    
    public ArticleManangerSucKhoeGiaDinh(IArticleSucKhoeGiaDinhRepository articleSucKhoeGiaDinhRepository, ICategorySucKhoeGiaDinhRepository categorySucKhoeGiaDinhRepository, IMediaSucKhoeGiaDinhRepository mediaSucKhoeGiaDinhRepository)
    {
        _articleSucKhoeGiaDinhRepository = articleSucKhoeGiaDinhRepository;
        _categorySucKhoeGiaDinhRepository = categorySucKhoeGiaDinhRepository;
        _mediaSucKhoeGiaDinhRepository = mediaSucKhoeGiaDinhRepository;
    }

    public async Task ProcessingDataAsync(List<ArticlePayload> articles)
    {
        var categories = await _categorySucKhoeGiaDinhRepository.GetListAsync();
        
        foreach (var article in articles)
        {
            var articleEntity = await _articleSucKhoeGiaDinhRepository.FirstOrDefaultAsync(x => x.Title.Equals(article.Title));
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
                //     await _categorySucKhoeGiaDinhRepository.InsertAsync(category, true);
                //     categories.Add(category);
                // }
                // articleEntity.AddCategory(category.Id);
            }
            else
            {
                articleEntity = new Article(GuidGenerator.Create())
                {
                    Title = article.Title,
                    CreatedAt = article.CreatedAt,
                    Excerpt = article.ShortDescription,
                    Content = article.Content,
                    Tags = article.Tags?.JoinAsString(";")
                };


                var category = categories.FirstOrDefault(x => x.Name == article.Category);
                if (category == null)
                {
                    category = new Category()
                    {
                        Name = article.Category
                    };
                    await _categorySucKhoeGiaDinhRepository.InsertAsync(category, true);
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
                    await _mediaSucKhoeGiaDinhRepository.InsertAsync(media, true);
                    articleEntity.FeaturedMediaId = media.Id;
                }
                
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
                        await _mediaSucKhoeGiaDinhRepository.InsertManyAsync(medias, true);

                        articleEntity.Content = StringHtmlHelper.SetContentMediaIds(article.Content, medias);
                        
                        foreach (var media in medias)
                        {
                            articleEntity.AddMedia(media.Id);
                        }
                    }
                }

                await _articleSucKhoeGiaDinhRepository.InsertAsync(articleEntity);
            }
        }
    }
    
}