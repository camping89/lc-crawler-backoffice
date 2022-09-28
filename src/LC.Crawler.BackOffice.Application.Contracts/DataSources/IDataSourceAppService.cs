using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace LC.Crawler.BackOffice.DataSources
{
    public interface IDataSourcesAppService : IApplicationService
    {
        Task<PagedResultDto<DataSourceDto>> GetListAsync(GetDataSourcesInput input);

        Task<DataSourceDto> GetAsync(Guid id);

        Task DeleteAsync(Guid id);

        Task<DataSourceDto> CreateAsync(DataSourceCreateDto input);

        Task<DataSourceDto> UpdateAsync(Guid id, DataSourceUpdateDto input);
    }
}