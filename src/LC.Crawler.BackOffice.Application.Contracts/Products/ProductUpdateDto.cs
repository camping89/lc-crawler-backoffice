using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities;

namespace LC.Crawler.BackOffice.Products
{
    public class ProductUpdateDto : IHasConcurrencyStamp
    {
        [Required]
        public string Name { get; set; }
        public string Code { get; set; }
        public string ShortDescription { get; set; }
        public string Description { get; set; }
        public int? ExternalId { get; set; }
        public Guid? FeaturedMediaId { get; set; }
        public Guid DataSourceId { get; set; }
        public List<Guid> CategoryIds { get; set; }
        public List<Guid> MediaIds { get; set; }

        public string ConcurrencyStamp { get; set; }
    }
}