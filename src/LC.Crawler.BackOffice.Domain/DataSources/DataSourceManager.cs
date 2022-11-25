using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using LC.Crawler.BackOffice.Enums;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Data;

namespace LC.Crawler.BackOffice.DataSources
{
    public class DataSourceManager : DomainService
    {
        private readonly IDataSourceRepository _dataSourceRepository;

        public DataSourceManager(IDataSourceRepository dataSourceRepository)
        {
            _dataSourceRepository = dataSourceRepository;
        }

        public async Task<DataSource> CreateAsync(string url, bool isActive, string postToSite,
            Configuration configuration)
        {
            var dataSource = new DataSource(GuidGenerator.Create(), url, isActive, postToSite, configuration);

            return await _dataSourceRepository.InsertAsync(dataSource);
        }

        public async Task<DataSource> UpdateAsync(Guid id, string url, bool isActive, string postToSite,
            Configuration configuration, [CanBeNull] string concurrencyStamp = null)
        {
            var queryable = await _dataSourceRepository.GetQueryableAsync();
            var query = queryable.Where(x => x.Id == id);

            var dataSource = await AsyncExecuter.FirstOrDefaultAsync(query);

            dataSource.Url = url;
            dataSource.IsActive = isActive;
            dataSource.PostToSite = postToSite;
            dataSource.Configuration = configuration;

            dataSource.SetConcurrencyStampIfNotNull(concurrencyStamp);
            return await _dataSourceRepository.UpdateAsync(dataSource);
        }
        
        public async Task DoUpdateSyncStatus(Guid id,PageSyncStatusType syncStatusType, PageSyncStatus status)
        {
            var queryable = await _dataSourceRepository.GetQueryableAsync();
            var query = queryable.Where(x => x.Id == id);

            var dataSource = await AsyncExecuter.FirstOrDefaultAsync(query);
            switch (syncStatusType)
            {
                case PageSyncStatusType.SyncProduct:
                    dataSource.ProductSyncStatus = status;
                    dataSource.LastProductSyncedAt = DateTime.UtcNow;
                    break;
                
                case PageSyncStatusType.ResyncProduct:
                    dataSource.ProductReSyncStatus = status;
                    dataSource.LastProductReSyncedAt = DateTime.UtcNow;
                    break;

                case PageSyncStatusType.SyncArticle:
                    dataSource.ArticleSyncStatus = status;
                    dataSource.LastArticleSyncedAt = DateTime.UtcNow;
                    break;
                
                case PageSyncStatusType.ResyncArticle:
                    dataSource.ArticleReSyncStatus = status;
                    dataSource.LastArticleReSyncedAt = DateTime.UtcNow;
                    break;

            }
            
            await _dataSourceRepository.UpdateAsync(dataSource, true);
        }
    }
}