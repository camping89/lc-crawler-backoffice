using LC.Crawler.BackOffice.Shared;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace LC.Crawler.BackOffice.ProductReviews
{
    public interface IProductReviewsAppService : IApplicationService
    {
        Task<PagedResultDto<ProductReviewWithNavigationPropertiesDto>> GetListAsync(GetProductReviewsInput input);

        Task<ProductReviewWithNavigationPropertiesDto> GetWithNavigationPropertiesAsync(Guid id);

        Task<ProductReviewDto> GetAsync(Guid id);

        Task<PagedResultDto<LookupDto<Guid>>> GetProductLookupAsync(LookupRequestDto input);

        Task DeleteAsync(Guid id);

        Task<ProductReviewDto> CreateAsync(ProductReviewCreateDto input);

        Task<ProductReviewDto> UpdateAsync(Guid id, ProductReviewUpdateDto input);
    }
}