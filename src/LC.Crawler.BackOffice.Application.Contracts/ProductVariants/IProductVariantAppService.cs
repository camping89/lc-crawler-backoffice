using LC.Crawler.BackOffice.Shared;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace LC.Crawler.BackOffice.ProductVariants
{
    public interface IProductVariantsAppService : IApplicationService
    {
        Task<PagedResultDto<ProductVariantWithNavigationPropertiesDto>> GetListAsync(GetProductVariantsInput input);

        Task<ProductVariantWithNavigationPropertiesDto> GetWithNavigationPropertiesAsync(Guid id);

        Task<ProductVariantDto> GetAsync(Guid id);

        Task<PagedResultDto<LookupDto<Guid>>> GetProductLookupAsync(LookupRequestDto input);

        Task DeleteAsync(Guid id);

        Task<ProductVariantDto> CreateAsync(ProductVariantCreateDto input);

        Task<ProductVariantDto> UpdateAsync(Guid id, ProductVariantUpdateDto input);
    }
}