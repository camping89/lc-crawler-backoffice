using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
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

        public async Task<DataSource> CreateAsync(
        string url, bool isActive)
        {
            var dataSource = new DataSource(
             GuidGenerator.Create(),
             url, isActive
             );

            return await _dataSourceRepository.InsertAsync(dataSource);
        }

        public async Task<DataSource> UpdateAsync(
            Guid id,
            string url, bool isActive, [CanBeNull] string concurrencyStamp = null
        )
        {
            var queryable = await _dataSourceRepository.GetQueryableAsync();
            var query = queryable.Where(x => x.Id == id);

            var dataSource = await AsyncExecuter.FirstOrDefaultAsync(query);

            dataSource.Url = url;
            dataSource.IsActive = isActive;

            dataSource.SetConcurrencyStampIfNotNull(concurrencyStamp);
            return await _dataSourceRepository.UpdateAsync(dataSource);
        }

        public Task<List<DataSource>> GetDataSourcesAsync()
        {
            return _dataSourceRepository.GetListAsync();
        }

    }
}