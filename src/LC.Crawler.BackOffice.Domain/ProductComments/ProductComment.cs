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

namespace LC.Crawler.BackOffice.ProductComments
{
    public class ProductComment : AuditedEntity<Guid>
    {
        [NotNull]
        public virtual string Name { get; set; }

        [CanBeNull]
        public virtual string Content { get; set; }

        public virtual int Likes { get; set; }

        public virtual DateTime? CreatedAt { get; set; }
        public Guid ProductId { get; set; }
        
        public bool IsSynced { get; set; }

        public ProductComment()
        {

        }

        public ProductComment(Guid id, Guid productId, string name, string content, int likes, DateTime? createdAt = null)
        {

            Id = id;
            Check.NotNull(name, nameof(name));
            Name = name;
            Content = content;
            Likes = likes;
            CreatedAt = createdAt;
            ProductId = productId;
        }

    }
}