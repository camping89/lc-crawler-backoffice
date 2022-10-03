using LC.Crawler.BackOffice.Medias;
using LC.Crawler.BackOffice.Categories;
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

namespace LC.Crawler.BackOffice.Products
{
    public class MongoProductRepository : MongoDbRepository<BackOfficeMongoDbContext, Product, Guid>, IProductRepository
    {
        public MongoProductRepository(IMongoDbContextProvider<BackOfficeMongoDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }

        public async Task<ProductWithNavigationProperties> GetWithNavigationPropertiesAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var product = await (await GetMongoQueryableAsync(cancellationToken))
                .FirstOrDefaultAsync(e => e.Id == id, GetCancellationToken(cancellationToken));

            var categoryIds = product.Categories.Select(x => x.CategoryId).ToList();
            var categories = await (await GetDbContextAsync(cancellationToken)).Categories.AsQueryable().Where(e => categoryIds.Contains(e.Id)).ToListAsync(cancellationToken: cancellationToken);
            var mediaIds = product.Medias.Select(x => x.MediaId).ToList();
            var medias = await (await GetDbContextAsync(cancellationToken)).Medias.AsQueryable().Where(e => mediaIds.Contains(e.Id)).ToListAsync(cancellationToken: cancellationToken);

            return new ProductWithNavigationProperties
            {
                Product = product,
                Categories = categories,
                Medias = medias,

            };
        }

        public async Task<List<ProductWithNavigationProperties>> GetListWithNavigationPropertiesAsync(
            string filterText = null,
            string name = null,
            string brand = null,
            double? ratingMin = null,
            double? ratingMax = null,
            decimal? priceMin = null,
            decimal? priceMax = null,
            double? discountPercentMin = null,
            double? discountPercentMax = null,
            string shortDescription = null,
            string description = null,
            Guid? categoryId = null,
            Guid? mediaId = null,
            string sorting = null,
            int maxResultCount = int.MaxValue,
            int skipCount = 0,
            CancellationToken cancellationToken = default)
        {
            var query = ApplyFilter((await GetMongoQueryableAsync(cancellationToken)), filterText, name, brand, ratingMin, ratingMax, priceMin, priceMax, discountPercentMin, discountPercentMax, shortDescription, description, categoryId, mediaId);
            var products = await query.OrderBy(string.IsNullOrWhiteSpace(sorting) ? ProductConsts.GetDefaultSorting(false) : sorting.Split('.').Last())
                .As<IMongoQueryable<Product>>()
                .PageBy<Product, IMongoQueryable<Product>>(skipCount, maxResultCount)
                .ToListAsync(GetCancellationToken(cancellationToken));

            var dbContext = await GetDbContextAsync(cancellationToken);
            return products.Select(s => new ProductWithNavigationProperties
            {
                Product = s,
                Categories = new List<Category>(),
                Medias = new List<Media>(),

            }).ToList();
        }

        public async Task<List<Product>> GetListAsync(
            string filterText = null,
            string name = null,
            string brand = null,
            double? ratingMin = null,
            double? ratingMax = null,
            decimal? priceMin = null,
            decimal? priceMax = null,
            double? discountPercentMin = null,
            double? discountPercentMax = null,
            string shortDescription = null,
            string description = null,
            string sorting = null,
            int maxResultCount = int.MaxValue,
            int skipCount = 0,
            CancellationToken cancellationToken = default)
        {
            var query = ApplyFilter((await GetMongoQueryableAsync(cancellationToken)), filterText, name, brand, ratingMin, ratingMax, priceMin, priceMax, discountPercentMin, discountPercentMax, shortDescription, description);
            query = query.OrderBy(string.IsNullOrWhiteSpace(sorting) ? ProductConsts.GetDefaultSorting(false) : sorting);
            return await query.As<IMongoQueryable<Product>>()
                .PageBy<Product, IMongoQueryable<Product>>(skipCount, maxResultCount)
                .ToListAsync(GetCancellationToken(cancellationToken));
        }

        public async Task<long> GetCountAsync(
           string filterText = null,
           string name = null,
           string brand = null,
           double? ratingMin = null,
           double? ratingMax = null,
           decimal? priceMin = null,
           decimal? priceMax = null,
           double? discountPercentMin = null,
           double? discountPercentMax = null,
           string shortDescription = null,
           string description = null,
           Guid? categoryId = null,
           Guid? mediaId = null,
           CancellationToken cancellationToken = default)
        {
            var query = ApplyFilter((await GetMongoQueryableAsync(cancellationToken)), filterText, name, brand, ratingMin, ratingMax, priceMin, priceMax, discountPercentMin, discountPercentMax, shortDescription, description, categoryId, mediaId);
            return await query.As<IMongoQueryable<Product>>().LongCountAsync(GetCancellationToken(cancellationToken));
        }

        protected virtual IQueryable<Product> ApplyFilter(
            IQueryable<Product> query,
            string filterText,
            string name = null,
            string brand = null,
            double? ratingMin = null,
            double? ratingMax = null,
            decimal? priceMin = null,
            decimal? priceMax = null,
            double? discountPercentMin = null,
            double? discountPercentMax = null,
            string shortDescription = null,
            string description = null,
            Guid? categoryId = null,
            Guid? mediaId = null)
        {
            return query
                .WhereIf(!string.IsNullOrWhiteSpace(filterText), e => e.Name.Contains(filterText) || e.Brand.Contains(filterText) || e.ShortDescription.Contains(filterText) || e.Description.Contains(filterText))
                    .WhereIf(!string.IsNullOrWhiteSpace(name), e => e.Name.Contains(name))
                    .WhereIf(!string.IsNullOrWhiteSpace(brand), e => e.Brand.Contains(brand))
                    .WhereIf(ratingMin.HasValue, e => e.Rating >= ratingMin.Value)
                    .WhereIf(ratingMax.HasValue, e => e.Rating <= ratingMax.Value)
                    .WhereIf(priceMin.HasValue, e => e.Price >= priceMin.Value)
                    .WhereIf(priceMax.HasValue, e => e.Price <= priceMax.Value)
                    .WhereIf(discountPercentMin.HasValue, e => e.DiscountPercent >= discountPercentMin.Value)
                    .WhereIf(discountPercentMax.HasValue, e => e.DiscountPercent <= discountPercentMax.Value)
                    .WhereIf(!string.IsNullOrWhiteSpace(shortDescription), e => e.ShortDescription.Contains(shortDescription))
                    .WhereIf(!string.IsNullOrWhiteSpace(description), e => e.Description.Contains(description))
                    .WhereIf(categoryId != null && categoryId != Guid.Empty, e => e.Categories.Any(x => x.CategoryId == categoryId))
                    .WhereIf(mediaId != null && mediaId != Guid.Empty, e => e.Medias.Any(x => x.MediaId == mediaId));
        }
    }
}