using System;
using LC.Crawler.BackOffice.PageDatasource.Aladin.MongoDb;
using LC.Crawler.BackOffice.ProductComments;
using Volo.Abp.Domain.Repositories.MongoDB;
using Volo.Abp.MongoDB;

namespace LC.Crawler.BackOffice.PageDatasource.Aladin.ProductComments
{
    public class MongoProductCommentAladinRepository : MongoDbRepository<AladinMongoDbContext, ProductComment, Guid>, IProductCommentAladinRepository
    {
        public MongoProductCommentAladinRepository(IMongoDbContextProvider<AladinMongoDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }

    }
}