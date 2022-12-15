using System;
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
using Microsoft.Extensions.Logging;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace LC.Crawler.BackOffice.Articles;

public class ArticleManangerLongChau : DomainService
{
    private readonly IArticleLongChauRepository _articleLongChauRepository;
    private readonly ICategoryLongChauRepository _categoryLongChauRepository;
    private readonly IMediaLongChauRepository _mediaLongChauRepository;
    private readonly IDataSourceRepository _dataSourceRepository;

    public ArticleManangerLongChau(IArticleLongChauRepository articleLongChauRepository,
        ICategoryLongChauRepository categoryLongChauRepository, IMediaLongChauRepository mediaLongChauRepository,
        IDataSourceRepository dataSourceRepository)
    {
        _articleLongChauRepository = articleLongChauRepository;
        _categoryLongChauRepository = categoryLongChauRepository;
        _mediaLongChauRepository = mediaLongChauRepository;
        _dataSourceRepository = dataSourceRepository;
    }

    public async Task ProcessingDataAsync(List<ArticlePayload> articles)
    {
        var dataSource = await _dataSourceRepository.GetAsync(x => x.Url.Contains(PageDataSourceConsts.LongChauUrl));
        if (dataSource == null)
        {
            return;
        }

        var entityMedias = await _mediaLongChauRepository.GetListAsync();
        var categories = await _categoryLongChauRepository.GetListAsync(x => x.CategoryType == CategoryType.Article);
        var articleGroup = articles.GroupBy(_ => _.Url).ToList();
        var index = 1;
        var total = articleGroup.Count();

        Console.WriteLine($"Start import");

        foreach (var rawArticles in articleGroup)
        {
            try
            {
                Console.WriteLine($"Processing {index}/{total}");

                var article = rawArticles.First();
                if (article.Content is null)
                {
                    continue;
                }

                var articleEntity =
                    await _articleLongChauRepository.FirstOrDefaultAsync(x => x.Title.Equals(article.Title));
                if (articleEntity == null)
                {
                    articleEntity = new Article(GuidGenerator.Create())
                    {
                        Title = article.Title,
                        CreatedAt = article.CreatedAt,
                        Excerpt = article.ShortDescription,
                        Content = article.Content,
                        DataSourceId = dataSource.Id,
                        Tags = article.Tags,
                        Url = article.Url
                    };

                    foreach (var raw in rawArticles)
                    {
                        if (!raw.Category.IsNotNullOrEmpty())
                        {
                            continue;
                        }

                        var category = categories.FirstOrDefault(x =>
                            x.Name.Trim().Replace(" ", string.Empty).Equals(
                                raw.Category.Trim().Replace(" ", string.Empty),
                                StringComparison.InvariantCultureIgnoreCase));
                        if (category == null)
                        {
                            category = new Category()
                            {
                                Name = raw.Category.Trim(),
                                CategoryType = CategoryType.Article
                            };
                            await _categoryLongChauRepository.InsertAsync(category, true);
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
                        await _mediaLongChauRepository.InsertAsync(media, true);
                        articleEntity.FeaturedMediaId = media.Id;
                    }

                    if (!string.IsNullOrEmpty(article.Content))
                    {
                        var mediaUrls = article.Content.GetImageUrls();

                        if (mediaUrls.Any())
                        {
                            var medias = mediaUrls.Select(url => new Media()
                            {
                                Url = url.Contains("http") ? url : $"{dataSource.Url}{url}",
                                IsDowloaded = false
                            }).ToList();
                            await _mediaLongChauRepository.InsertManyAsync(medias);

                            articleEntity.Content = StringHtmlHelper.SetContentMediaIds(article.Content, medias);

                            foreach (var media in medias)
                            {
                                articleEntity.AddMedia(media.Id);
                            }
                        }
                    }

                    await _articleLongChauRepository.InsertAsync(articleEntity);

                    await CheckFormatEntity(articleEntity);
                }
                else
                {
                    foreach (var raw in rawArticles)
                    {
                        if (!raw.Category.IsNotNullOrEmpty())
                        {
                            continue;
                        }

                        articleEntity.RemoveAllCategories();
                        var category = categories.FirstOrDefault(x =>
                            x.Name.Trim().Replace(" ", string.Empty).Equals(
                                raw.Category.Trim().Replace(" ", string.Empty),
                                StringComparison.InvariantCultureIgnoreCase));
                        if (category == null)
                        {
                            category = new Category()
                            {
                                Name = raw.Category.Trim(),
                                CategoryType = CategoryType.Article
                            };
                            await _categoryLongChauRepository.InsertAsync(category, true);
                            categories.Add(category);
                        }

                        articleEntity.AddCategory(category.Id);
                    }

                    articleEntity.Url ??= article.Url;

                    if (articleEntity.Content is null && !string.IsNullOrEmpty(article.Content))
                    {
                        var mediaUrls = article.Content.GetImageUrls();
                        var articleEntityMedias = entityMedias
                            .Where(_ => mediaUrls.Any(x => StringHtmlHelper.CompareUrls(_.Url, x))).ToList();
                        if (mediaUrls.Any())
                        {
                            var newMedias = mediaUrls
                                .Where(_ => !articleEntityMedias.Any(x => StringHtmlHelper.CompareUrls(_, x.Url)))
                                .Select(url => new Media()
                                {
                                    Url = url.Contains("http") ? url : $"{dataSource.Url}{url}",
                                    IsDowloaded = false
                                }).ToList();
                            await _mediaLongChauRepository.InsertManyAsync(newMedias);

                            articleEntityMedias.AddRange(newMedias);

                            articleEntity.Content =
                                StringHtmlHelper.SetContentMediaIds(article.Content, articleEntityMedias);

                            foreach (var media in newMedias)
                            {
                                articleEntity.AddMedia(media.Id);
                            }
                        }
                        else
                        {
                            articleEntity.Content = article.Content;
                        }
                    }

                    await _articleLongChauRepository.UpdateAsync(articleEntity);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            index++;
        }

        Console.WriteLine($"Finish import");
    }

    /// <summary>
    /// Remove the entity in case having format exception (unicode types ...)
    /// </summary>
    /// <param name="articleEntity"></param>
    private async Task CheckFormatEntity(Article articleEntity)
    {
        try
        {
            var checkArticle = await _articleLongChauRepository.GetAsync(articleEntity.Id);
        }
        catch (Exception e)
        {
            if (e.GetType() == typeof(FormatException))
            {
                Logger.LogException(e);
                await _articleLongChauRepository.DeleteAsync(articleEntity.Id);
            }
        }
    }

    public async Task<List<KeyValuePair<string, int>>> CountArticleByCategory()
    {
        var articles = await _articleLongChauRepository.GetListAsync();
        var categories = await _categoryLongChauRepository.GetListAsync(_ => _.CategoryType == CategoryType.Article);
        return categories.Select(category => new KeyValuePair<string, int>(category.Name,
            articles.Count(_ => _.Categories.Select(c => c.CategoryId).Contains(category.Id)))).ToList();
    }

    public async Task HandleDuplicateMedia()
    {
        var articles = await _articleLongChauRepository.GetListAsync(_ => _.Content != null && _.LastSyncedAt != null);
        var medias = await _mediaLongChauRepository.GetListAsync(_ => _.Url != null);
        foreach (var article in articles)
        {
            var articleMedias = medias.Where(_ =>  article.Medias.IsNotNullOrEmpty() && article.Medias.Select(x => x.MediaId).Contains(_.Id)).ToList();
            foreach (var articleMedia in articleMedias)
            {
                if (articleMedia.ExternalUrl is not null && articleMedia.ExternalId is not null) continue;

                var syncedMedia = medias.FirstOrDefault(_ => _.Url.IsNotNullOrEmpty() &&
                    _.Url.Equals(articleMedia.Url, StringComparison.InvariantCultureIgnoreCase) && _.ExternalUrl != null &&
                    _.ExternalId != null);
                if (syncedMedia is null) continue;

                article.RemoveMedia(articleMedia.Id);
                article.AddMedia(syncedMedia.Id);
                await _articleLongChauRepository.UpdateAsync(article, true);
                await _mediaLongChauRepository.DeleteAsync(articleMedia, true);
            }
        }
    }
    
    public async Task<List<string>> GetErrorEncodeData()
    {
        var errorIds = new List<string>();
        var articleIds = (await _articleLongChauRepository.GetQueryableAsync()).Select(x=>x.Id).ToList();
        foreach (var id in articleIds)
        {
            try
            {
                var article = await _articleLongChauRepository.GetAsync(id);
                if (!article.Medias.IsNotNullOrEmpty())
                {
                    continue;
                }
                foreach (var mediaId in article.Medias.Select(_ => _.MediaId).ToList())
                {
                    try
                    {
                        var media = await _mediaLongChauRepository.GetAsync(mediaId);
                    }
                    catch (Exception e)
                    {
                        errorIds.Add($"Message: {e.Message}");
                        errorIds.Add($"Error In Article Id: {id} ---- Media Id: {mediaId}");
                        Console.WriteLine($"");
                        Console.WriteLine(e.Message);
                        Console.WriteLine($"Media Id: {mediaId}");
                    }
                }
            }
            catch (Exception e)
            {
                errorIds.Add($"Message: {e.Message}");
                errorIds.Add($"Article Id: {id}");
                Console.WriteLine(e.Message);
                Console.WriteLine($"Article Id: {id}");
            }
        }
        return errorIds;
    }
}