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

namespace LC.Crawler.BackOffice.ProductVariants
{
    public class MongoProductVariantRepository : MongoDbRepository<BackOfficeMongoDbContext, ProductVariant, Guid>, IProductVariantRepository
    {
        public MongoProductVariantRepository(IMongoDbContextProvider<BackOfficeMongoDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }

        public async Task<ProductVariantWithNavigationProperties> GetWithNavigationPropertiesAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var productVariant = await (await GetMongoQueryableAsync(cancellationToken))
                .FirstOrDefaultAsync(e => e.Id == id, GetCancellationToken(cancellationToken));

            var product = await (await GetDbContextAsync(cancellationToken)).Products.AsQueryable().FirstOrDefaultAsync(e => e.Id == productVariant.ProductId, cancellationToken: cancellationToken);

            return new ProductVariantWithNavigationProperties
            {
                ProductVariant = productVariant,
                Product = product,

            };
        }

        public async Task<List<ProductVariantWithNavigationProperties>> GetListWithNavigationPropertiesAsync(
            string filterText = null,
            string sKU = null,
            decimal? retailPriceMin = null,
            decimal? retailPriceMax = null,
            double? discountRateMin = null,
            double? discountRateMax = null,
            decimal? discountedPriceMin = null,
            decimal? discountedPriceMax = null,
            Guid? productId = null,
            string sorting = null,
            int maxResultCount = int.MaxValue,
            int skipCount = 0,
            CancellationToken cancellationToken = default)
        {
            var query = ApplyFilter((await GetMongoQueryableAsync(cancellationToken)), filterText, sKU, retailPriceMin, retailPriceMax, discountRateMin, discountRateMax, discountedPriceMin, discountedPriceMax, productId);
            var productVariants = await query.OrderBy(string.IsNullOrWhiteSpace(sorting) ? ProductVariantConsts.GetDefaultSorting(false) : sorting.Split('.').Last())
                .As<IMongoQueryable<ProductVariant>>()
                .PageBy<ProductVariant, IMongoQueryable<ProductVariant>>(skipCount, maxResultCount)
                .ToListAsync(GetCancellationToken(cancellationToken));

            var dbContext = await GetDbContextAsync(cancellationToken);
            return productVariants.Select(s => new ProductVariantWithNavigationProperties
            {
                ProductVariant = s,
                Product = dbContext.Products.AsQueryable().FirstOrDefault(e => e.Id == s.ProductId),

            }).ToList();
        }

        public async Task<List<ProductVariant>> GetListAsync(
            string filterText = null,
            string sKU = null,
            decimal? retailPriceMin = null,
            decimal? retailPriceMax = null,
            double? discountRateMin = null,
            double? discountRateMax = null,
            decimal? discountedPriceMin = null,
            decimal? discountedPriceMax = null,
            string sorting = null,
            int maxResultCount = int.MaxValue,
            int skipCount = 0,
            CancellationToken cancellationToken = default)
        {
            var query = ApplyFilter((await GetMongoQueryableAsync(cancellationToken)), filterText, sKU, retailPriceMin, retailPriceMax, discountRateMin, discountRateMax, discountedPriceMin, discountedPriceMax);
            query = query.OrderBy(string.IsNullOrWhiteSpace(sorting) ? ProductVariantConsts.GetDefaultSorting(false) : sorting);
            return await query.As<IMongoQueryable<ProductVariant>>()
                .PageBy<ProductVariant, IMongoQueryable<ProductVariant>>(skipCount, maxResultCount)
                .ToListAsync(GetCancellationToken(cancellationToken));
        }

        public async Task<long> GetCountAsync(
           string filterText = null,
           string sKU = null,
           decimal? retailPriceMin = null,
           decimal? retailPriceMax = null,
           double? discountRateMin = null,
           double? discountRateMax = null,
           decimal? discountedPriceMin = null,
           decimal? discountedPriceMax = null,
           Guid? productId = null,
           CancellationToken cancellationToken = default)
        {
            var query = ApplyFilter((await GetMongoQueryableAsync(cancellationToken)), filterText, sKU, retailPriceMin, retailPriceMax, discountRateMin, discountRateMax, discountedPriceMin, discountedPriceMax, productId);
            return await query.As<IMongoQueryable<ProductVariant>>().LongCountAsync(GetCancellationToken(cancellationToken));
        }

        protected virtual IQueryable<ProductVariant> ApplyFilter(
            IQueryable<ProductVariant> query,
            string filterText,
            string sKU = null,
            decimal? retailPriceMin = null,
            decimal? retailPriceMax = null,
            double? discountRateMin = null,
            double? discountRateMax = null,
            decimal? discountedPriceMin = null,
            decimal? discountedPriceMax = null,
            Guid? productId = null)
        {
            return query
                .WhereIf(!string.IsNullOrWhiteSpace(filterText), e => e.SKU.Contains(filterText))
                    .WhereIf(!string.IsNullOrWhiteSpace(sKU), e => e.SKU.Contains(sKU))
                    .WhereIf(retailPriceMin.HasValue, e => e.RetailPrice >= retailPriceMin.Value)
                    .WhereIf(retailPriceMax.HasValue, e => e.RetailPrice <= retailPriceMax.Value)
                    .WhereIf(discountRateMin.HasValue, e => e.DiscountRate >= discountRateMin.Value)
                    .WhereIf(discountRateMax.HasValue, e => e.DiscountRate <= discountRateMax.Value)
                    .WhereIf(discountedPriceMin.HasValue, e => e.DiscountedPrice >= discountedPriceMin.Value)
                    .WhereIf(discountedPriceMax.HasValue, e => e.DiscountedPrice <= discountedPriceMax.Value)
                    .WhereIf(productId != null && productId != Guid.Empty, e => e.ProductId == productId);
        }
    }
}