using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using LC.Crawler.BackOffice.Categories;
using LC.Crawler.BackOffice.Enums;
using LC.Crawler.BackOffice.Extensions;
using LC.Crawler.BackOffice.Medias;
using LC.Crawler.BackOffice.PageDatasource.Aladin.MongoDb;
using LC.Crawler.BackOffice.Products;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Volo.Abp.Domain.Repositories.MongoDB;
using Volo.Abp.MongoDB;

namespace LC.Crawler.BackOffice.PageDatasource.Aladin.Products
{
    public class MongoProductAladinRepository : MongoDbRepository<AladinMongoDbContext, Product, Guid>, IProductAladinRepository
    {
        public MongoProductAladinRepository(IMongoDbContextProvider<AladinMongoDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }

        public async Task<ProductWithNavigationProperties> GetWithNavigationPropertiesAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var product = await (await GetMongoQueryableAsync(cancellationToken))
                .FirstOrDefaultAsync(e => e.Id == id, GetCancellationToken(cancellationToken));

            var media = await (await GetDbContextAsync(cancellationToken)).Medias.AsQueryable().FirstOrDefaultAsync(e => e.Id == product.FeaturedMediaId, cancellationToken: cancellationToken);
            //var dataSource = await (await GetDbContextAsync(cancellationToken)).DataSources.AsQueryable().FirstOrDefaultAsync(e => e.Id == product.DataSourceId, cancellationToken: cancellationToken);
            var categoryIds = product.Categories?.Select(x => x.CategoryId).ToList();
            var categories = new List<Category>();
            if (categoryIds.IsNotNullOrEmpty()) 
                categories = (await GetDbContextAsync(cancellationToken)).Categories.AsQueryable().WhereIf(categoryIds is { Count: > 0 } , e =>  categoryIds.Contains(e.Id)).ToList();
            
            var mediaIds = product.Medias?.Select(x => x.MediaId).ToList();
            var medias = new List<Media>();
            if (mediaIds.IsNotNullOrEmpty()) 
                medias = (await GetDbContextAsync(cancellationToken)).Medias.AsQueryable().WhereIf(mediaIds is { Count: > 0 } , e =>  mediaIds.Contains(e.Id)).ToList();
            
            var variants = await (await GetDbContextAsync(cancellationToken)).ProductVariants.AsQueryable().Where(e => e.ProductId == product.Id).ToListAsync(cancellationToken: cancellationToken);
            var attributes = await (await GetDbContextAsync(cancellationToken)).ProductAttributes.AsQueryable().Where(e => e.ProductId == product.Id).ToListAsync(cancellationToken: cancellationToken);

            return new ProductWithNavigationProperties
            {
                Product = product,
                Media = media,
                //DataSource = dataSource,
                Categories = categories,
                Medias = medias,
                Variants = variants,
                Attributes = attributes,
            };
        }

        public async Task<List<ProductWithNavigationProperties>> GetListWithNavigationPropertiesAsync(
            string filterText = null,
            string name = null,
            string code = null,
            string shortDescription = null,
            string description = null,
            int? externalIdMin = null,
            int? externalIdMax = null,
            Guid? featuredMediaId = null,
            Guid? dataSourceId = null,
            Guid? categoryId = null,
            Guid? mediaId = null,
            string sorting = null,
            int maxResultCount = int.MaxValue,
            int skipCount = 0,
            CancellationToken cancellationToken = default)
        {
            var query = ApplyFilter((await GetMongoQueryableAsync(cancellationToken)), filterText, name, code, shortDescription, description, externalIdMin, externalIdMax, featuredMediaId, dataSourceId, categoryId, mediaId);
            var products = await query.OrderBy(string.IsNullOrWhiteSpace(sorting) ? ProductConsts.GetDefaultSorting(false) : sorting.Split('.').Last())
                .As<IMongoQueryable<Product>>()
                .PageBy<Product, IMongoQueryable<Product>>(skipCount, maxResultCount)
                .ToListAsync(GetCancellationToken(cancellationToken));

            var dbContext = await GetDbContextAsync(cancellationToken);
            return products.Select(s => new ProductWithNavigationProperties
            {
                Product = s,
                Media = dbContext.Medias.AsQueryable().FirstOrDefault(e => e.Id == s.FeaturedMediaId),
                //DataSource = dbContext.DataSources.AsQueryable().FirstOrDefault(e => e.Id == s.DataSourceId),
                Categories = new List<Category>(),
                Medias = new List<Media>(),

            }).ToList();
        }

        public async Task<List<Product>> GetListAsync(
            string filterText = null,
            string name = null,
            string code = null,
            string shortDescription = null,
            string description = null,
            int? externalIdMin = null,
            int? externalIdMax = null,
            string sorting = null,
            int maxResultCount = int.MaxValue,
            int skipCount = 0,
            CancellationToken cancellationToken = default)
        {
            var query = ApplyFilter((await GetMongoQueryableAsync(cancellationToken)), filterText, name, code, shortDescription, description, externalIdMin, externalIdMax);
            query = query.OrderBy(string.IsNullOrWhiteSpace(sorting) ? ProductConsts.GetDefaultSorting(false) : sorting);
            return await query.As<IMongoQueryable<Product>>()
                .PageBy<Product, IMongoQueryable<Product>>(skipCount, maxResultCount)
                .ToListAsync(GetCancellationToken(cancellationToken));
        }

        public async Task<long> GetCountAsync(
           string filterText = null,
           string name = null,
           string code = null,
           string shortDescription = null,
           string description = null,
           int? externalIdMin = null,
           int? externalIdMax = null,
           Guid? featuredMediaId = null,
           Guid? dataSourceId = null,
           Guid? categoryId = null,
           Guid? mediaId = null,
           CancellationToken cancellationToken = default)
        {
            var query = ApplyFilter((await GetMongoQueryableAsync(cancellationToken)), filterText, name, code, shortDescription, description, externalIdMin, externalIdMax, featuredMediaId, dataSourceId, categoryId, mediaId);
            return await query.As<IMongoQueryable<Product>>().LongCountAsync(GetCancellationToken(cancellationToken));
        }

        protected virtual IQueryable<Product> ApplyFilter(
            IQueryable<Product> query,
            string filterText,
            string name = null,
            string code = null,
            string shortDescription = null,
            string description = null,
            int? externalIdMin = null,
            int? externalIdMax = null,
            Guid? featuredMediaId = null,
            Guid? dataSourceId = null,
            Guid? categoryId = null,
            Guid? mediaId = null)
        {
            return query
                .WhereIf(!string.IsNullOrWhiteSpace(filterText), e => e.Name.Contains(filterText) || e.Code.Contains(filterText) || e.ShortDescription.Contains(filterText) || e.Description.Contains(filterText))
                    .WhereIf(!string.IsNullOrWhiteSpace(name), e => e.Name.Contains(name))
                    .WhereIf(!string.IsNullOrWhiteSpace(code), e => e.Code.Contains(code))
                    .WhereIf(!string.IsNullOrWhiteSpace(shortDescription), e => e.ShortDescription.Contains(shortDescription))
                    .WhereIf(!string.IsNullOrWhiteSpace(description), e => e.Description.Contains(description))
                    .WhereIf(externalIdMin.HasValue, e => e.ExternalId >= externalIdMin.Value)
                    .WhereIf(externalIdMax.HasValue, e => e.ExternalId <= externalIdMax.Value)
                    .WhereIf(featuredMediaId != null && featuredMediaId != Guid.Empty, e => e.FeaturedMediaId == featuredMediaId)
                    .WhereIf(dataSourceId != null && dataSourceId != Guid.Empty, e => e.DataSourceId == dataSourceId)
                    .WhereIf(categoryId != null && categoryId != Guid.Empty, e => e.Categories.Any(x => x.CategoryId == categoryId))
                    .WhereIf(mediaId != null && mediaId != Guid.Empty, e => e.Medias.Any(x => x.MediaId == mediaId));
        }
    }
}