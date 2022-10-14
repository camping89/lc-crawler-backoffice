using System;
using LC.Crawler.BackOffice.PageDatasource.Aladin.MongoDb;
using LC.Crawler.BackOffice.ProductVariants;
using Volo.Abp.Domain.Repositories.MongoDB;
using Volo.Abp.MongoDB;

namespace LC.Crawler.BackOffice.PageDatasource.LongChau.ProductVariants
{
    public class MongoProductVariantLongChauRepository : MongoDbRepository<AladinMongoDbContext, ProductVariant, Guid>, IProductVariantLongChauRepository
    {
        public MongoProductVariantLongChauRepository(IMongoDbContextProvider<AladinMongoDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }
    }
}