using System;
using LC.Crawler.BackOffice.MongoDB;
using Volo.Abp.Domain.Repositories.MongoDB;
using Volo.Abp.MongoDB;

namespace LC.Crawler.BackOffice.TrackingDataSources;

public class MongoTrackingDataSourceRepository : MongoDbRepository<BackOfficeMongoDbContext, TrackingDataSource, Guid>, ITrackingDataSourceRepository
{
    public MongoTrackingDataSourceRepository(IMongoDbContextProvider<BackOfficeMongoDbContext> dbContextProvider) : base(dbContextProvider)
    {
    }
}