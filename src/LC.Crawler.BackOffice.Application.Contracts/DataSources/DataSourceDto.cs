using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;

namespace LC.Crawler.BackOffice.DataSources
{
    public class DataSourceDto : AuditedEntityDto<Guid>, IHasConcurrencyStamp
    {
        public string Url { get; set; }
        public bool IsActive { get; set; }
        public string PostToSite { get; set; }

        public string ConcurrencyStamp { get; set; }
    }
}