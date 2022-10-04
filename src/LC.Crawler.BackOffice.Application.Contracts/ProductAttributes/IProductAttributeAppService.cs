using LC.Crawler.BackOffice.Shared;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace LC.Crawler.BackOffice.ProductAttributes
{
    public interface IProductAttributesAppService : IApplicationService
    {
        Task<PagedResultDto<ProductAttributeWithNavigationPropertiesDto>> GetListAsync(GetProductAttributesInput input);

        Task<ProductAttributeWithNavigationPropertiesDto> GetWithNavigationPropertiesAsync(Guid id);

        Task<ProductAttributeDto> GetAsync(Guid id);

        Task<PagedResultDto<LookupDto<Guid>>> GetProductLookupAsync(LookupRequestDto input);

        Task DeleteAsync(Guid id);

        Task<ProductAttributeDto> CreateAsync(ProductAttributeCreateDto input);

        Task<ProductAttributeDto> UpdateAsync(Guid id, ProductAttributeUpdateDto input);
    }
}