using LC.Crawler.BackOffice.Shared;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace LC.Crawler.BackOffice.Products
{
    public interface IProductsAppService : IApplicationService
    {
        Task<PagedResultDto<ProductWithNavigationPropertiesDto>> GetListAsync(GetProductsInput input);

        Task<ProductWithNavigationPropertiesDto> GetWithNavigationPropertiesAsync(Guid id);

        Task<ProductDto> GetAsync(Guid id);

        Task<PagedResultDto<LookupDto<Guid?>>> GetMediaLookupAsync(LookupRequestDto input);

        Task<PagedResultDto<LookupDto<Guid>>> GetDataSourceLookupAsync(LookupRequestDto input);

        Task<PagedResultDto<LookupDto<Guid>>> GetCategoryLookupAsync(LookupRequestDto input);

        Task DeleteAsync(Guid id);

        Task<ProductDto> CreateAsync(ProductCreateDto input);

        Task<ProductDto> UpdateAsync(Guid id, ProductUpdateDto input);
    }
}