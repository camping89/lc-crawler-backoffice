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
        public ConfigurationDto Configuration { get; set; }
    }
    
    public class ConfigurationDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string ApiKey { get; set; }
        public string ApiSecret { get; set; }
    }
}