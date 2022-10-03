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

namespace LC.Crawler.BackOffice.Articles
{
    public class Article : AuditedEntity<Guid>, IHasConcurrencyStamp
    {
        [NotNull]
        public virtual string Title { get; set; }

        [CanBeNull]
        public virtual string Excerpt { get; set; }

        [CanBeNull]
        public virtual string Content { get; set; }

        public virtual DateTime CreatedAt { get; set; }

        [CanBeNull]
        public virtual string Author { get; set; }

        [CanBeNull]
        public virtual string Tags { get; set; }

        public virtual int LikeCount { get; set; }

        public virtual int CommentCount { get; set; }

        public virtual int ShareCount { get; set; }

        public ICollection<ArticleCategory> Categories { get; private set; }

        public string ConcurrencyStamp { get; set; }

        public Article()
        {

        }

        public Article(Guid id, string title, string excerpt, string content, DateTime createdAt, string author, string tags, int likeCount, int commentCount, int shareCount)
        {
            ConcurrencyStamp = Guid.NewGuid().ToString("N");
            Id = id;
            Check.NotNull(title, nameof(title));
            Title = title;
            Excerpt = excerpt;
            Content = content;
            CreatedAt = createdAt;
            Author = author;
            Tags = tags;
            LikeCount = likeCount;
            CommentCount = commentCount;
            ShareCount = shareCount;
            Categories = new Collection<ArticleCategory>();
        }
        public void AddCategory(Guid categoryId)
        {
            Check.NotNull(categoryId, nameof(categoryId));

            if (IsInCategories(categoryId))
            {
                return;
            }

            Categories.Add(new ArticleCategory(Id, categoryId));
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
            Categories.RemoveAll(x => x.ArticleId == Id);
        }

        private bool IsInCategories(Guid categoryId)
        {
            return Categories.Any(x => x.CategoryId == categoryId);
        }
    }
}