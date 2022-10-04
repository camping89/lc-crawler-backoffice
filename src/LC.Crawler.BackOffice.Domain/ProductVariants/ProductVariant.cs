using LC.Crawler.BackOffice.Products;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;
using JetBrains.Annotations;

using Volo.Abp;

namespace LC.Crawler.BackOffice.ProductVariants
{
    public class ProductVariant : AuditedEntity<Guid>
    {
        [CanBeNull]
        public virtual string SKU { get; set; }

        public virtual decimal RetailPrice { get; set; }

        public virtual double DiscountRate { get; set; }

        public virtual decimal DiscountedPrice { get; set; }
        public Guid ProductId { get; set; }

        public ProductVariant()
        {

        }

        public ProductVariant(Guid id, Guid productId, string sKU, decimal retailPrice, double discountRate, decimal discountedPrice)
        {

            Id = id;
            SKU = sKU;
            RetailPrice = retailPrice;
            DiscountRate = discountRate;
            DiscountedPrice = discountedPrice;
            ProductId = productId;
        }

    }
}