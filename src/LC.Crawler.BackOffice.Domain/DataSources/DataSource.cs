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

namespace LC.Crawler.BackOffice.DataSources
{
    public class DataSource : AuditedEntity<Guid>, IHasConcurrencyStamp
    {
        [NotNull]
        public virtual string Url { get; set; }

        public virtual bool IsActive { get; set; }

        [CanBeNull]
        public virtual string PostToSite { get; set; }

        public string ConcurrencyStamp { get; set; }

        public DataSource()
        {

        }

        public DataSource(Guid id, string url, bool isActive, string postToSite)
        {
            ConcurrencyStamp = Guid.NewGuid().ToString("N");
            Id = id;
            Check.NotNull(url, nameof(url));
            Url = url;
            IsActive = isActive;
            PostToSite = postToSite;
        }

    }
}