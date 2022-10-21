using LC.Crawler.BackOffice.Categories;
using LC.Crawler.BackOffice.Medias;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Data;

namespace LC.Crawler.BackOffice.Articles
{
    public class ArticleManager : DomainService
    {
        private readonly IArticleRepository _articleRepository;
        private readonly IRepository<Category, Guid> _categoryRepository;
        private readonly IRepository<Media, Guid> _mediaRepository;

        public ArticleManager(IArticleRepository articleRepository,
        IRepository<Category, Guid> categoryRepository,
        IRepository<Media, Guid> mediaRepository)
        {
            _articleRepository = articleRepository;
            _categoryRepository = categoryRepository;
            _mediaRepository = mediaRepository;
        }

        public async Task<Article> CreateAsync(
        List<Guid> categoryIds,
        List<Guid> mediaIds,
        Guid? featuredMediaId, Guid dataSourceId, string title, string excerpt, string content, DateTime createdAt, string author, List<string> tags, int likeCount, int commentCount, int shareCount)
        {
            var article = new Article(
             GuidGenerator.Create(),
             featuredMediaId, dataSourceId, title, excerpt, content, createdAt, author, tags, likeCount, commentCount, shareCount
             );

            await SetCategoriesAsync(article, categoryIds);
            await SetMediasAsync(article, mediaIds);

            return await _articleRepository.InsertAsync(article);
        }

        public async Task<Article> UpdateAsync(
            Guid id,
            List<Guid> categoryIds,
        List<Guid> mediaIds,
        Guid? featuredMediaId, Guid dataSourceId, string title, string excerpt, string content, DateTime createdAt, string author, List<string> tags, int likeCount, int commentCount, int shareCount, [CanBeNull] string concurrencyStamp = null
        )
        {
            var queryable = await _articleRepository.WithDetailsAsync(x => x.Categories, x => x.Medias);
            var query = queryable.Where(x => x.Id == id);

            var article = await AsyncExecuter.FirstOrDefaultAsync(query);

            article.FeaturedMediaId = featuredMediaId;
            article.DataSourceId = dataSourceId;
            article.Title = title;
            article.Excerpt = excerpt;
            article.Content = content;
            article.CreatedAt = createdAt;
            article.Author = author;
            article.Tags = tags;
            article.LikeCount = likeCount;
            article.CommentCount = commentCount;
            article.ShareCount = shareCount;

            await SetCategoriesAsync(article, categoryIds);
            await SetMediasAsync(article, mediaIds);

            article.SetConcurrencyStampIfNotNull(concurrencyStamp);
            return await _articleRepository.UpdateAsync(article);
        }

        private async Task SetCategoriesAsync(Article article, List<Guid> categoryIds)
        {
            if (categoryIds == null || !categoryIds.Any())
            {
                article.RemoveAllCategories();
                return;
            }

            var query = (await _categoryRepository.GetQueryableAsync())
                .Where(x => categoryIds.Contains(x.Id))
                .Select(x => x.Id);

            var categoryIdsInDb = await AsyncExecuter.ToListAsync(query);
            if (!categoryIdsInDb.Any())
            {
                return;
            }

            article.RemoveAllCategoriesExceptGivenIds(categoryIdsInDb);

            foreach (var categoryId in categoryIdsInDb)
            {
                article.AddCategory(categoryId);
            }
        }

        private async Task SetMediasAsync(Article article, List<Guid> mediaIds)
        {
            if (mediaIds == null || !mediaIds.Any())
            {
                article.RemoveAllMedias();
                return;
            }

            var query = (await _mediaRepository.GetQueryableAsync())
                .Where(x => mediaIds.Contains(x.Id))
                .Select(x => x.Id);

            var mediaIdsInDb = await AsyncExecuter.ToListAsync(query);
            if (!mediaIdsInDb.Any())
            {
                return;
            }

            article.RemoveAllMediasExceptGivenIds(mediaIdsInDb);

            foreach (var mediaId in mediaIdsInDb)
            {
                article.AddMedia(mediaId);
            }
        }

    }
}