using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;

namespace LC.Crawler.BackOffice.Products
{
    public class ProductDto : FullAuditedEntityDto<Guid>, IHasConcurrencyStamp
    {
        public string Name { get; set; }
        public string Brand { get; set; }
        public double Rating { get; set; }
        public decimal Price { get; set; }
        public double DiscountPercent { get; set; }
        public string ShortDescription { get; set; }
        public string Description { get; set; }

        public string ConcurrencyStamp { get; set; }
    }
}