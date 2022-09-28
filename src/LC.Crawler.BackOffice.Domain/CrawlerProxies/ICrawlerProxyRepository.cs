using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace LC.Crawler.BackOffice.CrawlerProxies
{
    public interface ICrawlerProxyRepository : IRepository<CrawlerProxy, Guid>
    {
        Task<List<CrawlerProxy>> GetListAsync(
            string filterText = null,
            string ip = null,
            string port = null,
            string protocol = null,
            string username = null,
            string password = null,
            DateTime? pingedAtMin = null,
            DateTime? pingedAtMax = null,
            bool? isActive = null,
            string sorting = null,
            int maxResultCount = int.MaxValue,
            int skipCount = 0,
            CancellationToken cancellationToken = default
        );

        Task<long> GetCountAsync(
            string filterText = null,
            string ip = null,
            string port = null,
            string protocol = null,
            string username = null,
            string password = null,
            DateTime? pingedAtMin = null,
            DateTime? pingedAtMax = null,
            bool? isActive = null,
            CancellationToken cancellationToken = default);
    }
}