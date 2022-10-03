using LC.Crawler.BackOffice.Categories;
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

        public ArticleManager(IArticleRepository articleRepository,
        IRepository<Category, Guid> categoryRepository)
        {
            _articleRepository = articleRepository;
            _categoryRepository = categoryRepository;
        }

        public async Task<Article> CreateAsync(
        List<Guid> categoryIds,
        string title, string excerpt, string content, DateTime createdAt, string author, string tags, int likeCount, int commentCount, int shareCount)
        {
            var article = new Article(
             GuidGenerator.Create(),
             title, excerpt, content, createdAt, author, tags, likeCount, commentCount, shareCount
             );

            await SetCategoriesAsync(article, categoryIds);

            return await _articleRepository.InsertAsync(article);
        }

        public async Task<Article> UpdateAsync(
            Guid id,
            List<Guid> categoryIds,
        string title, string excerpt, string content, DateTime createdAt, string author, string tags, int likeCount, int commentCount, int shareCount, [CanBeNull] string concurrencyStamp = null
        )
        {
            var queryable = await _articleRepository.WithDetailsAsync(x => x.Categories);
            var query = queryable.Where(x => x.Id == id);

            var article = await AsyncExecuter.FirstOrDefaultAsync(query);

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

    }
}