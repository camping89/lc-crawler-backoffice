using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;

namespace LC.Crawler.BackOffice.Products
{
    public class ProductDto : FullAuditedEntityDto<Guid>, IHasConcurrencyStamp
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public string ShortDescription { get; set; }
        public string Description { get; set; }
        public int? ExternalId { get; set; }
        public Guid? FeaturedMediaId { get; set; }
        public Guid DataSourceId { get; set; }

        public string ConcurrencyStamp { get; set; }
    }
}