using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using LC.Crawler.BackOffice.PageDatasource.Aladin.MongoDb;
using LC.Crawler.BackOffice.ProductAttributes;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Volo.Abp.Domain.Repositories.MongoDB;
using Volo.Abp.MongoDB;

namespace LC.Crawler.BackOffice.PageDatasource.Aladin.ProductAttributes
{
    public class MongoProductAttributeAladinRepository : MongoDbRepository<AladinMongoDbContext, ProductAttribute, Guid>, IProductAttributeAladinRepository
    {
        public MongoProductAttributeAladinRepository(IMongoDbContextProvider<AladinMongoDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }
    }
}