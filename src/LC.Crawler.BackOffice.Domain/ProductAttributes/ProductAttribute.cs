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

namespace LC.Crawler.BackOffice.ProductAttributes
{
    public class ProductAttribute : Entity<Guid>
    {
        [CanBeNull]
        public virtual string Slug { get; set; }

        [NotNull]
        public virtual string Key { get; set; }

        [NotNull]
        public virtual string Value { get; set; }
        public Guid ProductId { get; set; }

        public ProductAttribute()
        {

        }

        public ProductAttribute(Guid id, Guid productId, string slug, string key, string value)
        {

            Id = id;
            Check.NotNull(key, nameof(key));
            Check.NotNull(value, nameof(value));
            Slug = slug;
            Key = key;
            Value = value;
            ProductId = productId;
        }

    }
}