using LC.Crawler.BackOffice.Shared;
using LC.Crawler.BackOffice.Products;
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
using LC.Crawler.BackOffice.ProductReviews;

namespace LC.Crawler.BackOffice.ProductReviews
{

    [Authorize(BackOfficePermissions.ProductReviews.Default)]
    public class ProductReviewsAppService : ApplicationService, IProductReviewsAppService
    {
        private readonly IProductReviewRepository _productReviewRepository;
        private readonly ProductReviewManager _productReviewManager;
        private readonly IRepository<Product, Guid> _productRepository;

        public ProductReviewsAppService(IProductReviewRepository productReviewRepository, ProductReviewManager productReviewManager, IRepository<Product, Guid> productRepository)
        {
            _productReviewRepository = productReviewRepository;
            _productReviewManager = productReviewManager; _productRepository = productRepository;
        }

        public virtual async Task<PagedResultDto<ProductReviewWithNavigationPropertiesDto>> GetListAsync(GetProductReviewsInput input)
        {
            var totalCount = await _productReviewRepository.GetCountAsync(input.FilterText, input.Name, input.Content, input.RatingMin, input.RatingMax, input.CreatedAtMin, input.CreatedAtMax, input.LikesMin, input.LikesMax, input.ProductId);
            var items = await _productReviewRepository.GetListWithNavigationPropertiesAsync(input.FilterText, input.Name, input.Content, input.RatingMin, input.RatingMax, input.CreatedAtMin, input.CreatedAtMax, input.LikesMin, input.LikesMax, input.ProductId, input.Sorting, input.MaxResultCount, input.SkipCount);

            return new PagedResultDto<ProductReviewWithNavigationPropertiesDto>
            {
                TotalCount = totalCount,
                Items = ObjectMapper.Map<List<ProductReviewWithNavigationProperties>, List<ProductReviewWithNavigationPropertiesDto>>(items)
            };
        }

        public virtual async Task<ProductReviewWithNavigationPropertiesDto> GetWithNavigationPropertiesAsync(Guid id)
        {
            return ObjectMapper.Map<ProductReviewWithNavigationProperties, ProductReviewWithNavigationPropertiesDto>
                (await _productReviewRepository.GetWithNavigationPropertiesAsync(id));
        }

        public virtual async Task<ProductReviewDto> GetAsync(Guid id)
        {
            return ObjectMapper.Map<ProductReview, ProductReviewDto>(await _productReviewRepository.GetAsync(id));
        }

        public virtual async Task<PagedResultDto<LookupDto<Guid>>> GetProductLookupAsync(LookupRequestDto input)
        {
            var query = (await _productRepository.GetQueryableAsync())
                .WhereIf(!string.IsNullOrWhiteSpace(input.Filter),
                    x => x.Name != null &&
                         x.Name.Contains(input.Filter));

            var lookupData = await query.PageBy(input.SkipCount, input.MaxResultCount).ToDynamicListAsync<Product>();
            var totalCount = query.Count();
            return new PagedResultDto<LookupDto<Guid>>
            {
                TotalCount = totalCount,
                Items = ObjectMapper.Map<List<Product>, List<LookupDto<Guid>>>(lookupData)
            };
        }

        [Authorize(BackOfficePermissions.ProductReviews.Delete)]
        public virtual async Task DeleteAsync(Guid id)
        {
            await _productReviewRepository.DeleteAsync(id);
        }

        [Authorize(BackOfficePermissions.ProductReviews.Create)]
        public virtual async Task<ProductReviewDto> CreateAsync(ProductReviewCreateDto input)
        {
            if (input.ProductId == default)
            {
                throw new UserFriendlyException(L["The {0} field is required.", L["Product"]]);
            }

            var productReview = await _productReviewManager.CreateAsync(
            input.ProductId, input.Name, input.Content, input.Rating, input.Likes, input.CreatedAt
            );

            return ObjectMapper.Map<ProductReview, ProductReviewDto>(productReview);
        }

        [Authorize(BackOfficePermissions.ProductReviews.Edit)]
        public virtual async Task<ProductReviewDto> UpdateAsync(Guid id, ProductReviewUpdateDto input)
        {
            if (input.ProductId == default)
            {
                throw new UserFriendlyException(L["The {0} field is required.", L["Product"]]);
            }

            var productReview = await _productReviewManager.UpdateAsync(
            id,
            input.ProductId, input.Name, input.Content, input.Rating, input.Likes, input.CreatedAt
            );

            return ObjectMapper.Map<ProductReview, ProductReviewDto>(productReview);
        }
    }
}