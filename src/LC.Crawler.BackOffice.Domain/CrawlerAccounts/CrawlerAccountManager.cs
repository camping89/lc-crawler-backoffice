using System;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using LC.Crawler.BackOffice.Enums;
using Volo.Abp.Domain.Services;
using Volo.Abp.Data;

namespace LC.Crawler.BackOffice.CrawlerAccounts
{
    public class CrawlerAccountManager : DomainService
    {
        private readonly ICrawlerAccountRepository _crawlerAccountRepository;

        public CrawlerAccountManager(ICrawlerAccountRepository crawlerAccountRepository)
        {
            _crawlerAccountRepository = crawlerAccountRepository;
        }

        public async Task<CrawlerAccount> CreateAsync(
        string username, string password, string twoFactorCode, AccountType accountType, AccountStatus accountStatus, string email, string emailPassword, bool isActive)
        {
            var crawlerAccount = new CrawlerAccount(
             GuidGenerator.Create(),
             username, password, twoFactorCode, accountType, accountStatus, email, emailPassword, isActive
             );

            return await _crawlerAccountRepository.InsertAsync(crawlerAccount);
        }

        public async Task<CrawlerAccount> UpdateAsync(
            Guid id,
            string username, string password, string twoFactorCode, AccountType accountType, AccountStatus accountStatus, string email, string emailPassword, bool isActive, [CanBeNull] string concurrencyStamp = null
        )
        {
            var queryable = await _crawlerAccountRepository.GetQueryableAsync();
            var query = queryable.Where(x => x.Id == id);

            var crawlerAccount = await AsyncExecuter.FirstOrDefaultAsync(query);

            crawlerAccount.Username = username;
            crawlerAccount.Password = password;
            crawlerAccount.TwoFactorCode = twoFactorCode;
            crawlerAccount.AccountType = accountType;
            crawlerAccount.AccountStatus = accountStatus;
            crawlerAccount.Email = email;
            crawlerAccount.EmailPassword = emailPassword;
            crawlerAccount.IsActive = isActive;

            crawlerAccount.SetConcurrencyStampIfNotNull(concurrencyStamp);
            return await _crawlerAccountRepository.UpdateAsync(crawlerAccount);
        }
        public async Task UpdateAccountStatus(CrawlerAccount account)
        {
            if (account is not null)
            {
                var queryable = await _crawlerAccountRepository.GetQueryableAsync();
                var query = queryable.Where(x => x.Id == account.Id);

                var crawlerAccount = await AsyncExecuter.FirstOrDefaultAsync(query);

                if (crawlerAccount is not null)
                {
                    crawlerAccount.AccountStatus = account.AccountStatus;

                    await _crawlerAccountRepository.UpdateAsync(crawlerAccount);
                }
            }
            
        }
    }
}