using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using LC.Crawler.BackOffice.Articles;
using LC.Crawler.BackOffice.Categories;
using LC.Crawler.BackOffice.Extensions;
using LC.Crawler.BackOffice.Medias;
using LC.Crawler.BackOffice.PageDatasource.AloBacSi.MongoDb;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Volo.Abp.Domain.Repositories.MongoDB;
using Volo.Abp.MongoDB;

namespace LC.Crawler.BackOffice.PageDatasource.AloBacSi.Articles
{
    public class MongoArticleAloBacSiRepository : MongoDbRepository<AloBacSiMongoDbContext, Article, Guid>, IArticleAloBacSiRepository
    {
        public MongoArticleAloBacSiRepository(IMongoDbContextProvider<AloBacSiMongoDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }

        public async Task<ArticleWithNavigationProperties> GetWithNavigationPropertiesAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var article = await (await GetMongoQueryableAsync(cancellationToken))
                .FirstOrDefaultAsync(e => e.Id == id, GetCancellationToken(cancellationToken));

            var media = await (await GetDbContextAsync(cancellationToken)).Medias.AsQueryable().FirstOrDefaultAsync(e => e.Id == article.FeaturedMediaId, cancellationToken: cancellationToken);
            //var dataSource = await (await GetDbContextAsync(cancellationToken)).DataSources.AsQueryable().FirstOrDefaultAsync(e => e.Id == article.DataSourceId, cancellationToken: cancellationToken);
            
            var categoryIds = article.Categories?.Select(x => x.CategoryId).ToList();
            var categories = new List<Category>();
            if (categoryIds.IsNotNullOrEmpty()) 
                categories = (await GetDbContextAsync(cancellationToken)).Categories.AsQueryable().WhereIf(categoryIds is { Count: > 0 } , e =>  categoryIds.Contains(e.Id)).ToList();
            
            var mediaIds = article.Medias?.Select(x => x.MediaId).ToList();
            var medias = new List<Media>();
            if (mediaIds.IsNotNullOrEmpty()) 
                medias = (await GetDbContextAsync(cancellationToken)).Medias.AsQueryable().WhereIf(mediaIds is { Count: > 0 } , e =>  mediaIds.Contains(e.Id)).ToList();

            return new ArticleWithNavigationProperties
            {
                Article = article,
                Media = media,
                //DataSource = dataSource,
                Categories = categories,
                Medias = medias,

            };
        }

        public async Task<List<ArticleWithNavigationProperties>> GetListWithNavigationPropertiesAsync(
            string filterText = null,
            string title = null,
            string excerpt = null,
            string content = null,
            DateTime? createdAtMin = null,
            DateTime? createdAtMax = null,
            string author = null,
            string tags = null,
            int? likeCountMin = null,
            int? likeCountMax = null,
            int? commentCountMin = null,
            int? commentCountMax = null,
            int? shareCountMin = null,
            int? shareCountMax = null,
            Guid? featuredMediaId = null,
            Guid? dataSourceId = null,
            Guid? categoryId = null,
            Guid? mediaId = null,
            string sorting = null,
            int maxResultCount = int.MaxValue,
            int skipCount = 0,
            CancellationToken cancellationToken = default)
        {
            var query = ApplyFilter((await GetMongoQueryableAsync(cancellationToken)), filterText, title, excerpt, content, createdAtMin, createdAtMax, author, tags, likeCountMin, likeCountMax, commentCountMin, commentCountMax, shareCountMin, shareCountMax, featuredMediaId, dataSourceId, categoryId, mediaId);
            var articles = await query.OrderBy(string.IsNullOrWhiteSpace(sorting) ? ArticleConsts.GetDefaultSorting(false) : sorting.Split('.').Last())
                .As<IMongoQueryable<Article>>()
                .PageBy<Article, IMongoQueryable<Article>>(skipCount, maxResultCount)
                .ToListAsync(GetCancellationToken(cancellationToken));

            var dbContext = await GetDbContextAsync(cancellationToken);
            return articles.Select(s => new ArticleWithNavigationProperties
            {
                Article = s,
                Media = dbContext.Medias.AsQueryable().FirstOrDefault(e => e.Id == s.FeaturedMediaId),
                //DataSource = dbContext.DataSources.AsQueryable().FirstOrDefault(e => e.Id == s.DataSourceId),
                Categories = new List<Category>(),
                Medias = new List<Media>(),

            }).ToList();
        }

        public async Task<List<Article>> GetListAsync(
            string filterText = null,
            string title = null,
            string excerpt = null,
            string content = null,
            DateTime? createdAtMin = null,
            DateTime? createdAtMax = null,
            string author = null,
            string tags = null,
            int? likeCountMin = null,
            int? likeCountMax = null,
            int? commentCountMin = null,
            int? commentCountMax = null,
            int? shareCountMin = null,
            int? shareCountMax = null,
            string sorting = null,
            int maxResultCount = int.MaxValue,
            int skipCount = 0,
            CancellationToken cancellationToken = default)
        {
            var query = ApplyFilter((await GetMongoQueryableAsync(cancellationToken)), filterText, title, excerpt, content, createdAtMin, createdAtMax, author, tags, likeCountMin, likeCountMax, commentCountMin, commentCountMax, shareCountMin, shareCountMax);
            query = query.OrderBy(string.IsNullOrWhiteSpace(sorting) ? ArticleConsts.GetDefaultSorting(false) : sorting);
            return await query.As<IMongoQueryable<Article>>()
                .PageBy<Article, IMongoQueryable<Article>>(skipCount, maxResultCount)
                .ToListAsync(GetCancellationToken(cancellationToken));
        }

        public async Task<long> GetCountAsync(
           string filterText = null,
           string title = null,
           string excerpt = null,
           string content = null,
           DateTime? createdAtMin = null,
           DateTime? createdAtMax = null,
           string author = null,
           string tags = null,
           int? likeCountMin = null,
           int? likeCountMax = null,
           int? commentCountMin = null,
           int? commentCountMax = null,
           int? shareCountMin = null,
           int? shareCountMax = null,
           Guid? featuredMediaId = null,
           Guid? dataSourceId = null,
           Guid? categoryId = null,
           Guid? mediaId = null,
           CancellationToken cancellationToken = default)
        {
            var query = ApplyFilter((await GetMongoQueryableAsync(cancellationToken)), filterText, title, excerpt, content, createdAtMin, createdAtMax, author, tags, likeCountMin, likeCountMax, commentCountMin, commentCountMax, shareCountMin, shareCountMax, featuredMediaId, dataSourceId, categoryId, mediaId);
            return await query.As<IMongoQueryable<Article>>().LongCountAsync(GetCancellationToken(cancellationToken));
        }

        protected virtual IQueryable<Article> ApplyFilter(
            IQueryable<Article> query,
            string filterText,
            string title = null,
            string excerpt = null,
            string content = null,
            DateTime? createdAtMin = null,
            DateTime? createdAtMax = null,
            string author = null,
            string tags = null,
            int? likeCountMin = null,
            int? likeCountMax = null,
            int? commentCountMin = null,
            int? commentCountMax = null,
            int? shareCountMin = null,
            int? shareCountMax = null,
            Guid? featuredMediaId = null,
            Guid? dataSourceId = null,
            Guid? categoryId = null,
            Guid? mediaId = null)
        {
            return query
                .WhereIf(!string.IsNullOrWhiteSpace(filterText), e => e.Title.Contains(filterText) || e.Excerpt.Contains(filterText) || e.Content.Contains(filterText) || e.Author.Contains(filterText) || e.Tags.Contains(filterText))
                    .WhereIf(!string.IsNullOrWhiteSpace(title), e => e.Title.Contains(title))
                    .WhereIf(!string.IsNullOrWhiteSpace(excerpt), e => e.Excerpt.Contains(excerpt))
                    .WhereIf(!string.IsNullOrWhiteSpace(content), e => e.Content.Contains(content))
                    .WhereIf(createdAtMin.HasValue, e => e.CreatedAt >= createdAtMin.Value)
                    .WhereIf(createdAtMax.HasValue, e => e.CreatedAt <= createdAtMax.Value)
                    .WhereIf(!string.IsNullOrWhiteSpace(author), e => e.Author.Contains(author))
                    .WhereIf(!string.IsNullOrWhiteSpace(tags), e => e.Tags.Contains(tags))
                    .WhereIf(likeCountMin.HasValue, e => e.LikeCount >= likeCountMin.Value)
                    .WhereIf(likeCountMax.HasValue, e => e.LikeCount <= likeCountMax.Value)
                    .WhereIf(commentCountMin.HasValue, e => e.CommentCount >= commentCountMin.Value)
                    .WhereIf(commentCountMax.HasValue, e => e.CommentCount <= commentCountMax.Value)
                    .WhereIf(shareCountMin.HasValue, e => e.ShareCount >= shareCountMin.Value)
                    .WhereIf(shareCountMax.HasValue, e => e.ShareCount <= shareCountMax.Value)
                    .WhereIf(featuredMediaId != null && featuredMediaId != Guid.Empty, e => e.FeaturedMediaId == featuredMediaId)
                    .WhereIf(dataSourceId != null && dataSourceId != Guid.Empty, e => e.DataSourceId == dataSourceId)
                    .WhereIf(categoryId != null && categoryId != Guid.Empty, e => e.Categories.Any(x => x.CategoryId == categoryId))
                    .WhereIf(mediaId != null && mediaId != Guid.Empty, e => e.Medias.Any(x => x.MediaId == mediaId));
        }
    }
}