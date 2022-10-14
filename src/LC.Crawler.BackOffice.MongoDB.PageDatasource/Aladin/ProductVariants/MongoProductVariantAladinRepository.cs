using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using LC.Crawler.BackOffice.PageDatasource.Aladin.MongoDb;
using LC.Crawler.BackOffice.ProductVariants;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Volo.Abp.Domain.Repositories.MongoDB;
using Volo.Abp.MongoDB;

namespace LC.Crawler.BackOffice.PageDatasource.Aladin.ProductVariants
{
    public class MongoProductVariantAladinRepository : MongoDbRepository<AladinMongoDbContext, ProductVariant, Guid>, IProductVariantAladinRepository
    {
        public MongoProductVariantAladinRepository(IMongoDbContextProvider<AladinMongoDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }
    }
}