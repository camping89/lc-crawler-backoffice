using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace LC.Crawler.BackOffice.DataSources
{
    public class DataSourceCreateDto
    {
        [Required]
        public string Url { get; set; }
        public bool IsActive { get; set; }
    }
}