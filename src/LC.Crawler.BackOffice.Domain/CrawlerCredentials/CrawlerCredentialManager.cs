using System;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using LC.Crawler.BackOffice.Enums;
using Volo.Abp.Domain.Services;
using Volo.Abp.Data;

namespace LC.Crawler.BackOffice.CrawlerCredentials
{
    public class CrawlerCredentialManager : DomainService
    {
        private readonly ICrawlerCredentialRepository _crawlerCredentialRepository;

        public CrawlerCredentialManager(ICrawlerCredentialRepository crawlerCredentialRepository)
        {
            _crawlerCredentialRepository = crawlerCredentialRepository;
        }

        public async Task<CrawlerCredential> CreateAsync(
        Guid? crawlerAccountId, Guid? crawlerProxyId, DataSourceType dataSourceType, bool isAvailable, DateTime? crawledAt = null)
        {
            var crawlerCredential = new CrawlerCredential(
             GuidGenerator.Create(),
             crawlerAccountId, crawlerProxyId, dataSourceType, isAvailable, crawledAt
             );

            return await _crawlerCredentialRepository.InsertAsync(crawlerCredential);
        }

        public async Task<CrawlerCredential> UpdateAsync(
            Guid id,
            Guid? crawlerAccountId, Guid? crawlerProxyId, DataSourceType dataSourceType, bool isAvailable, DateTime? crawledAt = null, [CanBeNull] string concurrencyStamp = null
        )
        {
            var queryable = await _crawlerCredentialRepository.GetQueryableAsync();
            var query = queryable.Where(x => x.Id == id);

            var crawlerCredential = await AsyncExecuter.FirstOrDefaultAsync(query);

            crawlerCredential.CrawlerAccountId = crawlerAccountId;
            crawlerCredential.CrawlerProxyId = crawlerProxyId;
            crawlerCredential.DataSourceType = dataSourceType;
            crawlerCredential.IsAvailable = isAvailable;
            crawlerCredential.CrawledAt = crawledAt;

            crawlerCredential.SetConcurrencyStampIfNotNull(concurrencyStamp);
            return await _crawlerCredentialRepository.UpdateAsync(crawlerCredential);
        }

    }
}