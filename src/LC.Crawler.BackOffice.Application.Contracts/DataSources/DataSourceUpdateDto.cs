using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities;

namespace LC.Crawler.BackOffice.DataSources
{
    public class DataSourceUpdateDto : IHasConcurrencyStamp
    {
        [Required]
        public string Url { get; set; }
        public bool IsActive { get; set; }
        public string PostToSite { get; set; }

        public string ConcurrencyStamp { get; set; }
        public ConfigurationDto Configuration { get; set; }
    }
}