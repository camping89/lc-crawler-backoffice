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

public class ArticleManangerSongKhoeMedplus : DomainService
{
    private readonly IArticleSongKhoeMedplusRepository _articleSongKhoeMedplusRepository;
    private readonly ICategorySongKhoeMedplusRepository _categorySongKhoeMedplusRepository;
    private readonly IMediaSongKhoeMedplusRepository _mediaSongKhoeMedplusRepository;

    public ArticleManangerSongKhoeMedplus(IArticleSongKhoeMedplusRepository articleSongKhoeMedplusRepository, ICategorySongKhoeMedplusRepository categorySongKhoeMedplusRepository, IMediaSongKhoeMedplusRepository mediaSongKhoeMedplusRepository)
    {
        _articleSongKhoeMedplusRepository = articleSongKhoeMedplusRepository;
        _categorySongKhoeMedplusRepository = categorySongKhoeMedplusRepository;
        _mediaSongKhoeMedplusRepository = mediaSongKhoeMedplusRepository;
    }
    
    public async Task ProcessingDataAsync(List<ArticlePayload> articles)
    {
        var categories = await _categorySongKhoeMedplusRepository.GetListAsync();
        
        foreach (var article in articles)
        {
            var articleEntity = await _articleSongKhoeMedplusRepository.FirstOrDefaultAsync(x => x.Title.Equals(article.Title));
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
                //     await _categorySucKhoeDoiSongRepository.InsertAsync(category, true);
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
                    await _categorySongKhoeMedplusRepository.InsertAsync(category, true);
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
                    await _mediaSongKhoeMedplusRepository.InsertAsync(media, true);
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
                        await _mediaSongKhoeMedplusRepository.InsertManyAsync(medias, true);

                        articleEntity.Content = StringHtmlHelper.SetContentMediaIds(article.Content, medias);
                        
                        foreach (var media in medias)
                        {
                            articleEntity.AddMedia(media.Id);
                        }
                    }
                }

                await _articleSongKhoeMedplusRepository.InsertAsync(articleEntity);
            }
        }
    }
}