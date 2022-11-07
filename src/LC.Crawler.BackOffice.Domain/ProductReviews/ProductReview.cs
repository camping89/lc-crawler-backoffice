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

namespace LC.Crawler.BackOffice.ProductReviews
{
    public class ProductReview : AuditedEntity<Guid>
    {
        [CanBeNull]
        public virtual string Name { get; set; }

        [CanBeNull]
        public virtual string Content { get; set; }

        public virtual decimal Rating { get; set; }

        public virtual DateTime? CreatedAt { get; set; }

        public virtual int Likes { get; set; }
        public Guid ProductId { get; set; }
        
        public bool IsSynced { get; set; }

        public ProductReview()
        {

        }

        public ProductReview(Guid id, Guid productId, string name, string content, decimal rating, int likes, DateTime? createdAt = null)
        {

            Id = id;
            Name = name;
            Content = content;
            Rating = rating;
            Likes = likes;
            CreatedAt = createdAt;
            ProductId = productId;
        }

    }
}