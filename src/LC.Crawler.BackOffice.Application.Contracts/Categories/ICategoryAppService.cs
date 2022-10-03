using LC.Crawler.BackOffice.Shared;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace LC.Crawler.BackOffice.Categories
{
    public interface ICategoriesAppService : IApplicationService
    {
        Task<PagedResultDto<CategoryWithNavigationPropertiesDto>> GetListAsync(GetCategoriesInput input);

        Task<CategoryWithNavigationPropertiesDto> GetWithNavigationPropertiesAsync(Guid id);

        Task<CategoryDto> GetAsync(Guid id);

        Task<PagedResultDto<LookupDto<Guid?>>> GetCategoryLookupAsync(LookupRequestDto input);

        Task DeleteAsync(Guid id);

        Task<CategoryDto> CreateAsync(CategoryCreateDto input);

        Task<CategoryDto> UpdateAsync(Guid id, CategoryUpdateDto input);
    }
}