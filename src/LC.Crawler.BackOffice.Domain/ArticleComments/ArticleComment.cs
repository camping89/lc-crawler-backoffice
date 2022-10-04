using LC.Crawler.BackOffice.Articles;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;
using JetBrains.Annotations;

using Volo.Abp;

namespace LC.Crawler.BackOffice.ArticleComments
{
    public class ArticleComment : AuditedEntity<Guid>
    {
        [NotNull]
        public virtual string Name { get; set; }

        [CanBeNull]
        public virtual string Content { get; set; }

        public virtual int Likes { get; set; }

        public virtual DateTime? CreatedAt { get; set; }
        public Guid ArticleId { get; set; }

        public ArticleComment()
        {

        }

        public ArticleComment(Guid id, Guid articleId, string name, string content, int likes, DateTime? createdAt = null)
        {

            Id = id;
            Check.NotNull(name, nameof(name));
            Name = name;
            Content = content;
            Likes = likes;
            CreatedAt = createdAt;
            ArticleId = articleId;
        }

    }
}