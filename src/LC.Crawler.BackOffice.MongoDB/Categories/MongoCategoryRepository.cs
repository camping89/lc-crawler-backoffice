using LC.Crawler.BackOffice.Enums;
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

namespace LC.Crawler.BackOffice.Categories
{
    public class MongoCategoryRepository : MongoDbRepository<BackOfficeMongoDbContext, Category, Guid>, ICategoryRepository
    {
        public MongoCategoryRepository(IMongoDbContextProvider<BackOfficeMongoDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }

        public async Task<CategoryWithNavigationProperties> GetWithNavigationPropertiesAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var category = await (await GetMongoQueryableAsync(cancellationToken))
                .FirstOrDefaultAsync(e => e.Id == id, GetCancellationToken(cancellationToken));

            var category1 = await (await GetDbContextAsync(cancellationToken)).Categories.AsQueryable().FirstOrDefaultAsync(e => e.Id == category.ParentCategoryId, cancellationToken: cancellationToken);

            return new CategoryWithNavigationProperties
            {
                Category = category,
                Category1 = category1,

            };
        }

        public async Task<List<CategoryWithNavigationProperties>> GetListWithNavigationPropertiesAsync(
            string filterText = null,
            string name = null,
            string slug = null,
            string description = null,
            CategoryType? categoryType = null,
            Guid? parentCategoryId = null,
            string sorting = null,
            int maxResultCount = int.MaxValue,
            int skipCount = 0,
            CancellationToken cancellationToken = default)
        {
            var query = ApplyFilter((await GetMongoQueryableAsync(cancellationToken)), filterText, name, slug, description, categoryType, parentCategoryId);
            var categories = await query.OrderBy(string.IsNullOrWhiteSpace(sorting) ? CategoryConsts.GetDefaultSorting(false) : sorting.Split('.').Last())
                .As<IMongoQueryable<Category>>()
                .PageBy<Category, IMongoQueryable<Category>>(skipCount, maxResultCount)
                .ToListAsync(GetCancellationToken(cancellationToken));

            var dbContext = await GetDbContextAsync(cancellationToken);
            return categories.Select(s => new CategoryWithNavigationProperties
            {
                Category = s,
                Category1 = dbContext.Categories.AsQueryable().FirstOrDefault(e => e.Id == s.ParentCategoryId),

            }).ToList();
        }

        public async Task<List<Category>> GetListAsync(
            string filterText = null,
            string name = null,
            string slug = null,
            string description = null,
            CategoryType? categoryType = null,
            string sorting = null,
            int maxResultCount = int.MaxValue,
            int skipCount = 0,
            CancellationToken cancellationToken = default)
        {
            var query = ApplyFilter((await GetMongoQueryableAsync(cancellationToken)), filterText, name, slug, description, categoryType);
            query = query.OrderBy(string.IsNullOrWhiteSpace(sorting) ? CategoryConsts.GetDefaultSorting(false) : sorting);
            return await query.As<IMongoQueryable<Category>>()
                .PageBy<Category, IMongoQueryable<Category>>(skipCount, maxResultCount)
                .ToListAsync(GetCancellationToken(cancellationToken));
        }

        public async Task<long> GetCountAsync(
           string filterText = null,
           string name = null,
           string slug = null,
           string description = null,
           CategoryType? categoryType = null,
           Guid? parentCategoryId = null,
           CancellationToken cancellationToken = default)
        {
            var query = ApplyFilter((await GetMongoQueryableAsync(cancellationToken)), filterText, name, slug, description, categoryType, parentCategoryId);
            return await query.As<IMongoQueryable<Category>>().LongCountAsync(GetCancellationToken(cancellationToken));
        }

        protected virtual IQueryable<Category> ApplyFilter(
            IQueryable<Category> query,
            string filterText,
            string name = null,
            string slug = null,
            string description = null,
            CategoryType? categoryType = null,
            Guid? parentCategoryId = null)
        {
            return query
                .WhereIf(!string.IsNullOrWhiteSpace(filterText), e => e.Name.Contains(filterText) || e.Slug.Contains(filterText) || e.Description.Contains(filterText))
                    .WhereIf(!string.IsNullOrWhiteSpace(name), e => e.Name.Contains(name))
                    .WhereIf(!string.IsNullOrWhiteSpace(slug), e => e.Slug.Contains(slug))
                    .WhereIf(!string.IsNullOrWhiteSpace(description), e => e.Description.Contains(description))
                    .WhereIf(categoryType.HasValue, e => e.CategoryType == categoryType)
                    .WhereIf(parentCategoryId != null && parentCategoryId != Guid.Empty, e => e.ParentCategoryId == parentCategoryId);
        }
    }
}