using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using LC.Crawler.BackOffice.Permissions;
using LC.Crawler.BackOffice.DataSources;

namespace LC.Crawler.BackOffice.DataSources
{
    [Authorize(BackOfficePermissions.DataSources.Default)]
    public class DataSourcesAppService : ApplicationService, IDataSourcesAppService
    {
        private readonly IDataSourceRepository _dataSourceRepository;
        private readonly DataSourceManager _dataSourceManager;

        public DataSourcesAppService(IDataSourceRepository dataSourceRepository, DataSourceManager dataSourceManager)
        {
            _dataSourceRepository = dataSourceRepository;
            _dataSourceManager = dataSourceManager;
        }

        public virtual async Task<PagedResultDto<DataSourceDto>> GetListAsync(GetDataSourcesInput input)
        {
            var totalCount =
                await _dataSourceRepository.GetCountAsync(input.FilterText, input.Url, input.IsActive,
                    input.PostToSite);
            var items = await _dataSourceRepository.GetListAsync(input.FilterText, input.Url, input.IsActive,
                input.PostToSite, input.Sorting, input.MaxResultCount, input.SkipCount);

            return new PagedResultDto<DataSourceDto>
            {
                TotalCount = totalCount,
                Items = ObjectMapper.Map<List<DataSource>, List<DataSourceDto>>(items)
            };
        }

        public virtual async Task<DataSourceDto> GetAsync(Guid id)
        {
            return ObjectMapper.Map<DataSource, DataSourceDto>(await _dataSourceRepository.GetAsync(id));
        }

        [Authorize(BackOfficePermissions.DataSources.Delete)]
        public virtual async Task DeleteAsync(Guid id)
        {
            await _dataSourceRepository.DeleteAsync(id);
        }

        [Authorize(BackOfficePermissions.DataSources.Create)]
        public virtual async Task<DataSourceDto> CreateAsync(DataSourceCreateDto input)
        {
            var configuration = ObjectMapper.Map<ConfigurationDto, Configuration>(input.Configuration);
            var dataSource =
                await _dataSourceManager.CreateAsync(input.Url, input.IsActive, input.PostToSite, configuration);

            return ObjectMapper.Map<DataSource, DataSourceDto>(dataSource);
        }

        [Authorize(BackOfficePermissions.DataSources.Edit)]
        public virtual async Task<DataSourceDto> UpdateAsync(Guid id, DataSourceUpdateDto input)
        {
            var configuration = ObjectMapper.Map<ConfigurationDto, Configuration>(input.Configuration);
            var dataSource = await _dataSourceManager.UpdateAsync(id, input.Url, input.IsActive, input.PostToSite,
                configuration, input.ConcurrencyStamp);

            return ObjectMapper.Map<DataSource, DataSourceDto>(dataSource);
        }
    }
}