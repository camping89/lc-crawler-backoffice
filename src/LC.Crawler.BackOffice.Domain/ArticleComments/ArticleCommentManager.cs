using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace LC.Crawler.BackOffice.ArticleComments
{
    public class ArticleCommentManager : DomainService
    {
        private readonly IArticleCommentRepository _articleCommentRepository;

        public ArticleCommentManager(IArticleCommentRepository articleCommentRepository)
        {
            _articleCommentRepository = articleCommentRepository;
        }

        public async Task<ArticleComment> CreateAsync(
        Guid articleId, string name, string content, int likes, DateTime? createdAt = null)
        {
            var articleComment = new ArticleComment(
             GuidGenerator.Create(),
             articleId, name, content, likes, createdAt
             );

            return await _articleCommentRepository.InsertAsync(articleComment);
        }

        public async Task<ArticleComment> UpdateAsync(
            Guid id,
            Guid articleId, string name, string content, int likes, DateTime? createdAt = null
        )
        {
            var queryable = await _articleCommentRepository.GetQueryableAsync();
            var query = queryable.Where(x => x.Id == id);

            var articleComment = await AsyncExecuter.FirstOrDefaultAsync(query);

            articleComment.ArticleId = articleId;
            articleComment.Name = name;
            articleComment.Content = content;
            articleComment.Likes = likes;
            articleComment.CreatedAt = createdAt;

            return await _articleCommentRepository.UpdateAsync(articleComment);
        }

    }
}