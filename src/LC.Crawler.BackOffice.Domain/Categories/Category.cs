using System;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;
using JetBrains.Annotations;
using Volo.Abp;

namespace LC.Crawler.BackOffice.Categories
{
    public class Category : FullAuditedEntity<Guid>, IHasConcurrencyStamp
    {
        [NotNull]
        public virtual string Name { get; set; }

        [CanBeNull]
        public virtual string Slug { get; set; }

        [CanBeNull]
        public virtual string Description { get; set; }
        public Guid? ParentCategoryId { get; set; }

        public string ConcurrencyStamp { get; set; }

        public Category()
        {

        }

        public Category(Guid id, Guid? parentCategoryId, string name, string slug, string description)
        {
            ConcurrencyStamp = Guid.NewGuid().ToString("N");
            Id = id;
            Check.NotNull(name, nameof(name));
            Name = name;
            Slug = slug;
            Description = description;
            ParentCategoryId = parentCategoryId;
        }

    }
}