using LC.Crawler.BackOffice.Shared;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace LC.Crawler.BackOffice.ProductComments
{
    public interface IProductCommentsAppService : IApplicationService
    {
        Task<PagedResultDto<ProductCommentWithNavigationPropertiesDto>> GetListAsync(GetProductCommentsInput input);

        Task<ProductCommentWithNavigationPropertiesDto> GetWithNavigationPropertiesAsync(Guid id);

        Task<ProductCommentDto> GetAsync(Guid id);

        Task<PagedResultDto<LookupDto<Guid>>> GetProductLookupAsync(LookupRequestDto input);

        Task DeleteAsync(Guid id);

        Task<ProductCommentDto> CreateAsync(ProductCommentCreateDto input);

        Task<ProductCommentDto> UpdateAsync(Guid id, ProductCommentUpdateDto input);
    }
}