using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using LC.Crawler.BackOffice.Configs;
using LC.Crawler.BackOffice.CrawlerAccounts;
using LC.Crawler.BackOffice.CrawlerProxies;
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
        
        
        public async Task UpdateMany(List<CrawlerCredential> crawlerCredentials)
        {
            var queryable = await _crawlerCredentialRepository.GetQueryableAsync();
            var ids = crawlerCredentials.Select(credential => credential.Id);
            var query = queryable.Where(x => ids.Contains(x.Id));
            var crawlerCredentialsInDatabase = await AsyncExecuter.ToListAsync(query);
            foreach (var crawlerCredentialInDatabase in crawlerCredentialsInDatabase)
            {
                var crawlerCredential = crawlerCredentials.FirstOrDefault(credential => credential.Id == crawlerCredentialInDatabase.Id);

                if (crawlerCredential != null)
                {
                    crawlerCredentialInDatabase.CrawlerAccountId = crawlerCredential.CrawlerAccountId;
                    crawlerCredentialInDatabase.CrawlerProxyId = crawlerCredential.CrawlerProxyId;
                    crawlerCredentialInDatabase.DataSourceType = crawlerCredential.DataSourceType;
                    crawlerCredentialInDatabase.IsAvailable = crawlerCredential.IsAvailable;
                }
            }

            await _crawlerCredentialRepository.UpdateManyAsync(crawlerCredentialsInDatabase);
        }

        public async Task<List<CrawlerCredentialWithNavigationProperties>> GetValidCredentials()
        {
            var crawlerCredentialWithNavigationProperties = await _crawlerCredentialRepository.GetListWithNavigationPropertiesAsync();
            crawlerCredentialWithNavigationProperties = crawlerCredentialWithNavigationProperties.Where(properties =>
                properties.CrawlerCredential.IsAvailable &&
                properties.CrawlerAccount.AccountStatus is AccountStatus.Active or AccountStatus.Ready &&
                properties.CrawlerProxy.IsActive).ToList();
            return crawlerCredentialWithNavigationProperties.OrderBy(properties => properties.CrawlerCredential.CrawledAt).ToList();
        }

        public async Task ResetCrawlerInformation(CrawlerCredential credential)
        {
            if (credential is not null)
            {
                var queryable = await _crawlerCredentialRepository.GetQueryableAsync();
                var query = queryable.Where(x => x.Id == credential.Id);

                var crawlerCredential = await AsyncExecuter.FirstOrDefaultAsync(query);

                if (crawlerCredential is not null)
                {
                    crawlerCredential.IsAvailable = true;
                    crawlerCredential.CrawledAt = credential.CrawledAt;
                    await _crawlerCredentialRepository.UpdateAsync(crawlerCredential);
                }
            }
        }

        public async Task Rebind(List<CrawlerAccount> crawlerAccounts, List<CrawlerProxy> crawlerProxies)
        {
            var crawlerProxyIds = crawlerProxies.Select(proxy => proxy.Id).ToList();
            var crawlerAccountIds = crawlerAccounts.Select(account => account.Id).ToList();
            var existingCrawlerCredentials = await _crawlerCredentialRepository.GetListAsync(credential => credential.IsDeleted == false);

            var deletingCrawlerCredentials = existingCrawlerCredentials
                .Where(credential => credential.CrawlerProxyId.HasValue && !crawlerProxyIds.Contains(credential.CrawlerProxyId.Value)).ToList();

            var deletingCrawlerAccount = existingCrawlerCredentials.Where(credential =>  credential.CrawlerProxyId.HasValue &&  credential.CrawlerAccountId.HasValue && 
                                                                                         crawlerProxyIds.Contains(credential.CrawlerProxyId.Value) &&
                                                                                         !crawlerAccountIds.Contains(credential.CrawlerAccountId.Value));

            deletingCrawlerCredentials = deletingCrawlerCredentials.Union(deletingCrawlerAccount).ToList();
            var aliveCrawlerCredentials = existingCrawlerCredentials.Except(deletingCrawlerCredentials).ToList();

            if (deletingCrawlerCredentials.Any())
            {
                Parallel.ForEach(deletingCrawlerCredentials, crawlerCredential => { crawlerCredential.ConcurrencyStamp ??= Guid.NewGuid().ToString("N"); });
                
                await _crawlerCredentialRepository.DeleteManyAsync(deletingCrawlerCredentials);
            }

            await RebindNewCrawlerCredentials(crawlerAccounts, aliveCrawlerCredentials, crawlerProxyIds);
        }

        private async Task RebindNewCrawlerCredentials(List<CrawlerAccount> crawlerAccounts, List<CrawlerCredential> aliveCrawlerCredentials, List<Guid> crawlerProxyIds)
        {
            var groupCrawlerCredentials = aliveCrawlerCredentials.Where(c =>  c.CrawlerProxyId.HasValue ).GroupBy(credential => credential.CrawlerProxyId.Value)
                .Select(grouping => new {grouping.Key, CrawlerProxies = grouping.ToList()}).ToList();
            
            // collect crawlerAccounts: crawlerAccount is account that is not used in aliveCrawlerCredentials
            var usingCrawlerAccountIds = aliveCrawlerCredentials.Where(c =>  c.CrawlerAccountId.HasValue ).Select(credential => credential.CrawlerAccountId.Value).Distinct().ToList();
            crawlerAccounts = crawlerAccounts.Where(account => !usingCrawlerAccountIds.Contains(account.Id)).ToList();

            var newCrawlerCredentials = new List<CrawlerCredential>();
            foreach (var crawlerProxyId in crawlerProxyIds)
            {
                if (!crawlerAccounts.Any())
                {
                    continue;
                }

                var groupCrawlerCredential = groupCrawlerCredentials.FirstOrDefault(arg => arg.Key == crawlerProxyId);
                var newCrawlerAccounts = groupCrawlerCredential is not null
                    ? crawlerAccounts.Take(GlobalConfig.Crawler.CrawlerAccountPerProxy - groupCrawlerCredential.CrawlerProxies.Count).ToList()
                    : crawlerAccounts.Take(GlobalConfig.Crawler.CrawlerAccountPerProxy).ToList();

                crawlerAccounts = crawlerAccounts.Except(newCrawlerAccounts).ToList();

                foreach (var newCrawlerAccount in newCrawlerAccounts)
                {
                    var crawlerCredential = new CrawlerCredential
                    {
                        IsAvailable = true,
                        CrawlerAccountId = newCrawlerAccount.Id,
                        CrawlerProxyId = crawlerProxyId,
                        DataSourceType = GetDataSourceType(newCrawlerAccount.AccountType),
                        ConcurrencyStamp = Guid.NewGuid().ToString("N")
                    };
                    
                    newCrawlerCredentials.Add(crawlerCredential);
                    
                }
            }

            if (newCrawlerCredentials.Any())
            {
                await _crawlerCredentialRepository.InsertManyAsync(newCrawlerCredentials);
            }
        }

        private DataSourceType GetDataSourceType(AccountType accountType)
        {
            return accountType switch
            {
                AccountType.FacebookGroupPost => DataSourceType.Group,
                AccountType.Faire => DataSourceType.Website,
                _ => throw new ArgumentOutOfRangeException(nameof(accountType), accountType, null)
            };
        }

    }
}