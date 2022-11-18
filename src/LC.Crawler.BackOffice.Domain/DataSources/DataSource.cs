using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using FluentDate;
using FluentDateTime;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;
using JetBrains.Annotations;
using LC.Crawler.BackOffice.Configs;
using LC.Crawler.BackOffice.Enums;
using Volo.Abp.Domain.Entities;

using Volo.Abp;

namespace LC.Crawler.BackOffice.DataSources
{
    public class DataSource : AuditedEntity<Guid>, IHasConcurrencyStamp
    {
        [NotNull]
        public virtual string Url { get; set; }

        public virtual bool IsActive { get; set; }

        [CanBeNull]
        public virtual string PostToSite { get; set; }

        public string ConcurrencyStamp { get; set; }
        public Configuration Configuration { get; set; }

        public DateTime?      LastCrawledAt  { get; set; }
        public PageSyncStatus CrawlerStatus  { get; set; }

        public DateTime?      LastProductSyncedAt   { get; set; }
        public PageSyncStatus ProductSyncStatus     { get; set; }
        
        public bool ShouldSyncProduct
        {
            get
            {
                {
                    var isObsolete = LastProductSyncedAt.HasValue && LastProductSyncedAt.Value.IsBefore(GlobalConfig.Crawler.SyncTimeOutInHours.Hours().Ago());
                    return ProductSyncStatus != PageSyncStatus.InProgress || isObsolete;
                }
            }
        }
        
        public DateTime?      LastArticleSyncedAt { get; set; }
        public PageSyncStatus ArticleSyncStatus   { get; set; }
        
        public bool ShouldSyncArticle
        {
            get
            {
                {
                    var isObsolete = LastArticleSyncedAt.HasValue && LastArticleSyncedAt.Value.IsBefore(GlobalConfig.Crawler.SyncTimeOutInHours.Hours().Ago());
                    return ArticleSyncStatus != PageSyncStatus.InProgress || isObsolete;
                }
            }
        }
        
        public DateTime?      LastProductReSyncedAt { get; set; }
        public PageSyncStatus ProductReSyncStatus   { get; set; }
        
        public bool ShouldReSyncProduct
        {
            get
            {
                {
                    var isObsolete = LastProductReSyncedAt.HasValue && LastProductReSyncedAt.Value.IsBefore(GlobalConfig.Crawler.SyncTimeOutInHours.Hours().Ago());
                    return ProductReSyncStatus != PageSyncStatus.InProgress || isObsolete;
                }
            }
        }
        
        public DateTime?      LastArticleReSyncedAt { get; set; }
        public PageSyncStatus ArticleReSyncStatus   { get; set; }
        
        public bool ShouldReSyncArticle
        {
            get
            {
                {
                    var isObsolete = LastArticleReSyncedAt.HasValue && LastArticleReSyncedAt.Value.IsBefore(GlobalConfig.Crawler.SyncTimeOutInHours.Hours().Ago());
                    return ArticleReSyncStatus != PageSyncStatus.InProgress || isObsolete;
                }
            }
        }

        public DataSource()
        {

        }

        public DataSource(Guid id, string url, bool isActive, string postToSite, Configuration configuration)
        {
            ConcurrencyStamp = Guid.NewGuid().ToString("N");
            Id = id;
            Check.NotNull(url, nameof(url));
            Url = url;
            IsActive = isActive;
            PostToSite = postToSite;
            Configuration = configuration;
        }

    }

    public class Configuration
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string ApiKey { get; set; }
        public string ApiSecret { get; set; }
    }
}