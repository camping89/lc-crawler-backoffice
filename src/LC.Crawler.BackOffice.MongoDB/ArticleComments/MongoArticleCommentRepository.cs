using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using LC.Crawler.BackOffice.MongoDB;
using Volo.Abp.Domain.Repositories.MongoDB;
using Volo.Abp.MongoDB;
using MongoDB.Driver.Linq;
using MongoDB.Driver;

namespace LC.Crawler.BackOffice.ArticleComments
{
    public class MongoArticleCommentRepository : MongoDbRepository<BackOfficeMongoDbContext, ArticleComment, Guid>, IArticleCommentRepository
    {
        public MongoArticleCommentRepository(IMongoDbContextProvider<BackOfficeMongoDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }

        public async Task<ArticleCommentWithNavigationProperties> GetWithNavigationPropertiesAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var articleComment = await (await GetMongoQueryableAsync(cancellationToken))
                .FirstOrDefaultAsync(e => e.Id == id, GetCancellationToken(cancellationToken));

            var article = await (await GetDbContextAsync(cancellationToken)).Articles.AsQueryable().FirstOrDefaultAsync(e => e.Id == articleComment.ArticleId, cancellationToken: cancellationToken);

            return new ArticleCommentWithNavigationProperties
            {
                ArticleComment = articleComment,
                Article = article,

            };
        }

        public async Task<List<ArticleCommentWithNavigationProperties>> GetListWithNavigationPropertiesAsync(
            string filterText = null,
            string name = null,
            string content = null,
            int? likesMin = null,
            int? likesMax = null,
            DateTime? createdAtMin = null,
            DateTime? createdAtMax = null,
            Guid? articleId = null,
            string sorting = null,
            int maxResultCount = int.MaxValue,
            int skipCount = 0,
            CancellationToken cancellationToken = default)
        {
            var query = ApplyFilter((await GetMongoQueryableAsync(cancellationToken)), filterText, name, content, likesMin, likesMax, createdAtMin, createdAtMax, articleId);
            var articleComments = await query.OrderBy(string.IsNullOrWhiteSpace(sorting) ? ArticleCommentConsts.GetDefaultSorting(false) : sorting.Split('.').Last())
                .As<IMongoQueryable<ArticleComment>>()
                .PageBy<ArticleComment, IMongoQueryable<ArticleComment>>(skipCount, maxResultCount)
                .ToListAsync(GetCancellationToken(cancellationToken));

            var dbContext = await GetDbContextAsync(cancellationToken);
            return articleComments.Select(s => new ArticleCommentWithNavigationProperties
            {
                ArticleComment = s,
                Article = dbContext.Articles.AsQueryable().FirstOrDefault(e => e.Id == s.ArticleId),

            }).ToList();
        }

        public async Task<List<ArticleComment>> GetListAsync(
            string filterText = null,
            string name = null,
            string content = null,
            int? likesMin = null,
            int? likesMax = null,
            DateTime? createdAtMin = null,
            DateTime? createdAtMax = null,
            string sorting = null,
            int maxResultCount = int.MaxValue,
            int skipCount = 0,
            CancellationToken cancellationToken = default)
        {
            var query = ApplyFilter((await GetMongoQueryableAsync(cancellationToken)), filterText, name, content, likesMin, likesMax, createdAtMin, createdAtMax);
            query = query.OrderBy(string.IsNullOrWhiteSpace(sorting) ? ArticleCommentConsts.GetDefaultSorting(false) : sorting);
            return await query.As<IMongoQueryable<ArticleComment>>()
                .PageBy<ArticleComment, IMongoQueryable<ArticleComment>>(skipCount, maxResultCount)
                .ToListAsync(GetCancellationToken(cancellationToken));
        }

        public async Task<long> GetCountAsync(
           string filterText = null,
           string name = null,
           string content = null,
           int? likesMin = null,
           int? likesMax = null,
           DateTime? createdAtMin = null,
           DateTime? createdAtMax = null,
           Guid? articleId = null,
           CancellationToken cancellationToken = default)
        {
            var query = ApplyFilter((await GetMongoQueryableAsync(cancellationToken)), filterText, name, content, likesMin, likesMax, createdAtMin, createdAtMax, articleId);
            return await query.As<IMongoQueryable<ArticleComment>>().LongCountAsync(GetCancellationToken(cancellationToken));
        }

        protected virtual IQueryable<ArticleComment> ApplyFilter(
            IQueryable<ArticleComment> query,
            string filterText,
            string name = null,
            string content = null,
            int? likesMin = null,
            int? likesMax = null,
            DateTime? createdAtMin = null,
            DateTime? createdAtMax = null,
            Guid? articleId = null)
        {
            return query
                .WhereIf(!string.IsNullOrWhiteSpace(filterText), e => e.Name.Contains(filterText) || e.Content.Contains(filterText))
                    .WhereIf(!string.IsNullOrWhiteSpace(name), e => e.Name.Contains(name))
                    .WhereIf(!string.IsNullOrWhiteSpace(content), e => e.Content.Contains(content))
                    .WhereIf(likesMin.HasValue, e => e.Likes >= likesMin.Value)
                    .WhereIf(likesMax.HasValue, e => e.Likes <= likesMax.Value)
                    .WhereIf(createdAtMin.HasValue, e => e.CreatedAt >= createdAtMin.Value)
                    .WhereIf(createdAtMax.HasValue, e => e.CreatedAt <= createdAtMax.Value)
                    .WhereIf(articleId != null && articleId != Guid.Empty, e => e.ArticleId == articleId);
        }
    }
}