using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace LC.Crawler.BackOffice.DataSources
{
    public interface IDataSourceRepository : IRepository<DataSource, Guid>
    {
        Task<List<DataSource>> GetListAsync(
            string filterText = null,
            string url = null,
            bool? isActive = null,
            string postToSite = null,
            string sorting = null,
            int maxResultCount = int.MaxValue,
            int skipCount = 0,
            CancellationToken cancellationToken = default
        );

        Task<long> GetCountAsync(
            string filterText = null,
            string url = null,
            bool? isActive = null,
            string postToSite = null,
            CancellationToken cancellationToken = default);
    }
}