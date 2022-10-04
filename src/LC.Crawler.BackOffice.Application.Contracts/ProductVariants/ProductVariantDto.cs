using System;
using Volo.Abp.Application.Dtos;

namespace LC.Crawler.BackOffice.ProductVariants
{
    public class ProductVariantDto : AuditedEntityDto<Guid>
    {
        public string SKU { get; set; }
        public decimal RetailPrice { get; set; }
        public double DiscountRate { get; set; }
        public decimal DiscountedPrice { get; set; }
        public Guid ProductId { get; set; }

    }
}