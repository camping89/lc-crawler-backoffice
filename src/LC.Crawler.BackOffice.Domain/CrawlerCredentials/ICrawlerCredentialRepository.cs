using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LC.Crawler.BackOffice.Enums;
using Volo.Abp.Domain.Repositories;

namespace LC.Crawler.BackOffice.CrawlerCredentials
{
    public interface ICrawlerCredentialRepository : IRepository<CrawlerCredential, Guid>
    {
        Task<CrawlerCredentialWithNavigationProperties> GetWithNavigationPropertiesAsync(
    Guid id,
    CancellationToken cancellationToken = default
);

        Task<List<CrawlerCredentialWithNavigationProperties>> GetListWithNavigationPropertiesAsync(
            string filterText = null,
            DataSourceType? dataSourceType = null,
            DateTime? crawledAtMin = null,
            DateTime? crawledAtMax = null,
            bool? isAvailable = null,
            Guid? crawlerAccountId = null,
            Guid? crawlerProxyId = null,
            string sorting = null,
            int maxResultCount = int.MaxValue,
            int skipCount = 0,
            CancellationToken cancellationToken = default
        );

        Task<List<CrawlerCredential>> GetListAsync(
                    string filterText = null,
                    DataSourceType? dataSourceType = null,
                    DateTime? crawledAtMin = null,
                    DateTime? crawledAtMax = null,
                    bool? isAvailable = null,
                    string sorting = null,
                    int maxResultCount = int.MaxValue,
                    int skipCount = 0,
                    CancellationToken cancellationToken = default
                );

        Task<long> GetCountAsync(
            string filterText = null,
            DataSourceType? dataSourceType = null,
            DateTime? crawledAtMin = null,
            DateTime? crawledAtMax = null,
            bool? isAvailable = null,
            Guid? crawlerAccountId = null,
            Guid? crawlerProxyId = null,
            CancellationToken cancellationToken = default);
    }
}