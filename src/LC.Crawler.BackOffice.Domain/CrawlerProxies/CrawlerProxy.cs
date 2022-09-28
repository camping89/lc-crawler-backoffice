using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;
using JetBrains.Annotations;
using Volo.Abp.Domain.Entities;

using Volo.Abp;

namespace LC.Crawler.BackOffice.CrawlerProxies
{
    public class CrawlerProxy : AuditedEntity<Guid>, IHasConcurrencyStamp
    {
        [NotNull]
        public virtual string Ip { get; set; }

        [CanBeNull]
        public virtual string Port { get; set; }

        [NotNull]
        public virtual string Protocol { get; set; }

        [CanBeNull]
        public virtual string Username { get; set; }

        [CanBeNull]
        public virtual string Password { get; set; }

        public virtual DateTime? PingedAt { get; set; }

        public virtual bool IsActive { get; set; }

        public string ConcurrencyStamp { get; set; }

        public CrawlerProxy()
        {

        }

        public CrawlerProxy(Guid id, string ip, string port, string protocol, string username, string password, bool isActive, DateTime? pingedAt = null)
        {
            ConcurrencyStamp = Guid.NewGuid().ToString("N");
            Id = id;
            Check.NotNull(ip, nameof(ip));
            Check.NotNull(protocol, nameof(protocol));
            Ip = ip;
            Port = port;
            Protocol = protocol;
            Username = username;
            Password = password;
            IsActive = isActive;
            PingedAt = pingedAt;
        }

    }
}