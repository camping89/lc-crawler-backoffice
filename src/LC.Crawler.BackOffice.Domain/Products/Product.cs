using LC.Crawler.BackOffice.Medias;
using LC.Crawler.BackOffice.DataSources;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;
using JetBrains.Annotations;
using Volo.Abp.Domain.Entities;

using Volo.Abp;

namespace LC.Crawler.BackOffice.Products
{
    public class Product : FullAuditedEntity<Guid>, IHasConcurrencyStamp
    {
        [NotNull]
        public virtual string Name { get; set; }

        [CanBeNull]
        public virtual string Code { get; set; }

        [CanBeNull]
        public virtual string ShortDescription { get; set; }

        [CanBeNull]
        public virtual string Description { get; set; }
        public string Brand { get; set; }

        public virtual int? ExternalId { get; set; }
        public Guid? FeaturedMediaId { get; set; }
        public Guid DataSourceId { get; set; }
        public ICollection<ProductCategory> Categories { get; private set; }
        public ICollection<ProductMedia> Medias { get; private set; }
        
        public virtual List<string> Tags { get; set; }

        public string ConcurrencyStamp { get; set; }
        public string Url { get; set; }

        public Product()
        {

        }

        public Product(Guid id)
        {
            Id = id;
            ConcurrencyStamp = Guid.NewGuid().ToString("N");
        }

        public Product(Guid id, Guid? featuredMediaId, Guid dataSourceId, string name, string code, string shortDescription, string description, int? externalId = null)
        {
            ConcurrencyStamp = Guid.NewGuid().ToString("N");
            Id = id;
            Check.NotNull(name, nameof(name));
            Name = name;
            Code = code;
            ShortDescription = shortDescription;
            Description = description;
            ExternalId = externalId;
            FeaturedMediaId = featuredMediaId;
            DataSourceId = dataSourceId;
            Categories = new Collection<ProductCategory>();
            Medias = new Collection<ProductMedia>();
        }
        public void AddCategory(Guid categoryId)
        {
            Check.NotNull(categoryId, nameof(categoryId));
            Categories ??= new List<ProductCategory>();
            if (IsInCategories(categoryId))
            {
                return;
            }

            Categories.Add(new ProductCategory(Id, categoryId));
        }

        public void RemoveCategory(Guid categoryId)
        {
            Check.NotNull(categoryId, nameof(categoryId));

            if (!IsInCategories(categoryId))
            {
                return;
            }

            Categories.RemoveAll(x => x.CategoryId == categoryId);
        }

        public void RemoveAllCategoriesExceptGivenIds(List<Guid> categoryIds)
        {
            Check.NotNullOrEmpty(categoryIds, nameof(categoryIds));

            Categories.RemoveAll(x => !categoryIds.Contains(x.CategoryId));
        }

        public void RemoveAllCategories()
        {
            Categories.RemoveAll(x => x.ProductId == Id);
        }

        private bool IsInCategories(Guid categoryId)
        {
            return Categories.Any(x => x.CategoryId == categoryId);
        }

        public void AddMedia(Guid mediaId)
        {
            Check.NotNull(mediaId, nameof(mediaId));
            Medias ??= new List<ProductMedia>();
            if (IsInMedias(mediaId))
            {
                return;
            }

            Medias.Add(new ProductMedia(Id, mediaId));
        }

        public void RemoveMedia(Guid mediaId)
        {
            Check.NotNull(mediaId, nameof(mediaId));

            if (!IsInMedias(mediaId))
            {
                return;
            }

            Medias.RemoveAll(x => x.MediaId == mediaId);
        }

        public void RemoveAllMediasExceptGivenIds(List<Guid> mediaIds)
        {
            Check.NotNullOrEmpty(mediaIds, nameof(mediaIds));

            Medias.RemoveAll(x => !mediaIds.Contains(x.MediaId));
        }

        public void RemoveAllMedias()
        {
            Medias.RemoveAll(x => x.ProductId == Id);
        }

        private bool IsInMedias(Guid mediaId)
        {
            return Medias.Any(x => x.MediaId == mediaId);
        }
    }
}