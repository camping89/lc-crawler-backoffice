using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;
using JetBrains.Annotations;
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
        
        public virtual List<string> Tags { get; set; }

        public virtual int LikeCount { get; set; }

        public virtual int CommentCount { get; set; }

        public virtual int ShareCount { get; set; }
        public Guid? FeaturedMediaId { get; set; }
        public Guid DataSourceId { get; set; }
        public ICollection<ArticleCategory> Categories { get; private set; }
        public ICollection<ArticleMedia> Medias { get; private set; }

        public string ConcurrencyStamp { get; set; }
        
        public DateTime? LastSyncedAt { get; set; }

        public Article(Guid id)
        {
            ConcurrencyStamp = Guid.NewGuid().ToString("N");
            Id = id;
        }

        public Article(Guid id, Guid? featuredMediaId, Guid dataSourceId, string title, string excerpt, string content, DateTime createdAt, string author, List<string> tags, int likeCount, int commentCount, int shareCount)
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
            FeaturedMediaId = featuredMediaId;
            DataSourceId = dataSourceId;
            Categories = new Collection<ArticleCategory>();
            Medias = new Collection<ArticleMedia>();
        }
        public void AddCategory(Guid categoryId)
        {
            Check.NotNull(categoryId, nameof(categoryId));
            Categories ??= new List<ArticleCategory>();
            
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
            if (Categories != null)
            {
                Categories.RemoveAll(x => x.ArticleId == Id);
            }
        }

        private bool IsInCategories(Guid categoryId)
        {
            return Categories.Any(x => x.CategoryId == categoryId);
        }

        public void AddMedia(Guid mediaId)
        {
            Check.NotNull(mediaId, nameof(mediaId));

            if (IsInMedias(mediaId))
            {
                return;
            }

            if (Medias != null)
            {
                Medias.Add(new ArticleMedia(Id, mediaId));
            }
            else
            {
                Medias = new List<ArticleMedia>() { new ArticleMedia(Id, mediaId) };
            }
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
            Medias.RemoveAll(x => x.ArticleId == Id);
        }

        private bool IsInMedias(Guid mediaId)
        {
            return Medias != null && Medias.Any(x => x.MediaId == mediaId);
        }
    }
}