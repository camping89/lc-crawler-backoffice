using System;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;
using JetBrains.Annotations;
using LC.Crawler.BackOffice.Enums;
using Volo.Abp;

namespace LC.Crawler.BackOffice.CrawlerAccounts
{
    public class CrawlerAccount : AuditedEntity<Guid>, IHasConcurrencyStamp
    {
        [NotNull]
        public virtual string Username { get; set; }

        [NotNull]
        public virtual string Password { get; set; }

        [NotNull]
        public virtual string TwoFactorCode { get; set; }

        public virtual AccountType AccountType { get; set; }

        public virtual AccountStatus AccountStatus { get; set; }

        [NotNull]
        public virtual string Email { get; set; }

        [NotNull]
        public virtual string EmailPassword { get; set; }

        public virtual bool IsActive { get; set; }

        public string ConcurrencyStamp { get; set; }

        public CrawlerAccount()
        {

        }

        public CrawlerAccount(Guid id, string username, string password, string twoFactorCode, AccountType accountType, AccountStatus accountStatus, string email, string emailPassword, bool isActive)
        {
            ConcurrencyStamp = Guid.NewGuid().ToString("N");
            Id = id;
            Check.NotNull(username, nameof(username));
            Check.NotNull(password, nameof(password));
            Check.NotNull(twoFactorCode, nameof(twoFactorCode));
            Check.NotNull(email, nameof(email));
            Check.NotNull(emailPassword, nameof(emailPassword));
            Username = username;
            Password = password;
            TwoFactorCode = twoFactorCode;
            AccountType = accountType;
            AccountStatus = accountStatus;
            Email = email;
            EmailPassword = emailPassword;
            IsActive = isActive;
        }

    }
}