using System;
using Volo.Abp.Domain.Repositories;

namespace LC.Crawler.BackOffice.TrackingDataSources;

public interface ITrackingDataSourceRepository : IRepository<TrackingDataSource, Guid>
{
    
}