using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace LC.Crawler.BackOffice.ProductVariants
{
    public class ProductVariantUpdateDto
    {
        public string SKU { get; set; }
        public decimal RetailPrice { get; set; }
        public double DiscountRate { get; set; }
        public decimal DiscountedPrice { get; set; }
        public Guid ProductId { get; set; }

    }
}