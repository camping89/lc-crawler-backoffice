using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Data;

namespace LC.Crawler.BackOffice.CrawlerProxies
{
    public class CrawlerProxyManager : DomainService
    {
        private readonly ICrawlerProxyRepository _crawlerProxyRepository;

        public CrawlerProxyManager(ICrawlerProxyRepository crawlerProxyRepository)
        {
            _crawlerProxyRepository = crawlerProxyRepository;
        }

        public async Task<CrawlerProxy> CreateAsync(
        string ip, string port, string protocol, string username, string password, bool isActive, DateTime? pingedAt = null)
        {
            var crawlerProxy = new CrawlerProxy(
             GuidGenerator.Create(),
             ip, port, protocol, username, password, isActive, pingedAt
             );

            return await _crawlerProxyRepository.InsertAsync(crawlerProxy);
        }

        public async Task<CrawlerProxy> UpdateAsync(
            Guid id,
            string ip, string port, string protocol, string username, string password, bool isActive, DateTime? pingedAt = null, [CanBeNull] string concurrencyStamp = null
        )
        {
            var queryable = await _crawlerProxyRepository.GetQueryableAsync();
            var query = queryable.Where(x => x.Id == id);

            var crawlerProxy = await AsyncExecuter.FirstOrDefaultAsync(query);

            crawlerProxy.Ip = ip;
            crawlerProxy.Port = port;
            crawlerProxy.Protocol = protocol;
            crawlerProxy.Username = username;
            crawlerProxy.Password = password;
            crawlerProxy.IsActive = isActive;
            crawlerProxy.PingedAt = pingedAt;

            crawlerProxy.SetConcurrencyStampIfNotNull(concurrencyStamp);
            return await _crawlerProxyRepository.UpdateAsync(crawlerProxy);
        }

    }
}