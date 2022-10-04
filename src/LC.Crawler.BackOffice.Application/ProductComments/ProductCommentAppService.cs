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
using LC.Crawler.BackOffice.ProductComments;

namespace LC.Crawler.BackOffice.ProductComments
{

    [Authorize(BackOfficePermissions.ProductComments.Default)]
    public class ProductCommentsAppService : ApplicationService, IProductCommentsAppService
    {
        private readonly IProductCommentRepository _productCommentRepository;
        private readonly ProductCommentManager _productCommentManager;
        private readonly IRepository<Product, Guid> _productRepository;

        public ProductCommentsAppService(IProductCommentRepository productCommentRepository, ProductCommentManager productCommentManager, IRepository<Product, Guid> productRepository)
        {
            _productCommentRepository = productCommentRepository;
            _productCommentManager = productCommentManager; _productRepository = productRepository;
        }

        public virtual async Task<PagedResultDto<ProductCommentWithNavigationPropertiesDto>> GetListAsync(GetProductCommentsInput input)
        {
            var totalCount = await _productCommentRepository.GetCountAsync(input.FilterText, input.Name, input.Content, input.LikesMin, input.LikesMax, input.CreatedAtMin, input.CreatedAtMax, input.ProductId);
            var items = await _productCommentRepository.GetListWithNavigationPropertiesAsync(input.FilterText, input.Name, input.Content, input.LikesMin, input.LikesMax, input.CreatedAtMin, input.CreatedAtMax, input.ProductId, input.Sorting, input.MaxResultCount, input.SkipCount);

            return new PagedResultDto<ProductCommentWithNavigationPropertiesDto>
            {
                TotalCount = totalCount,
                Items = ObjectMapper.Map<List<ProductCommentWithNavigationProperties>, List<ProductCommentWithNavigationPropertiesDto>>(items)
            };
        }

        public virtual async Task<ProductCommentWithNavigationPropertiesDto> GetWithNavigationPropertiesAsync(Guid id)
        {
            return ObjectMapper.Map<ProductCommentWithNavigationProperties, ProductCommentWithNavigationPropertiesDto>
                (await _productCommentRepository.GetWithNavigationPropertiesAsync(id));
        }

        public virtual async Task<ProductCommentDto> GetAsync(Guid id)
        {
            return ObjectMapper.Map<ProductComment, ProductCommentDto>(await _productCommentRepository.GetAsync(id));
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

        [Authorize(BackOfficePermissions.ProductComments.Delete)]
        public virtual async Task DeleteAsync(Guid id)
        {
            await _productCommentRepository.DeleteAsync(id);
        }

        [Authorize(BackOfficePermissions.ProductComments.Create)]
        public virtual async Task<ProductCommentDto> CreateAsync(ProductCommentCreateDto input)
        {
            if (input.ProductId == default)
            {
                throw new UserFriendlyException(L["The {0} field is required.", L["Product"]]);
            }

            var productComment = await _productCommentManager.CreateAsync(
            input.ProductId, input.Name, input.Content, input.Likes, input.CreatedAt
            );

            return ObjectMapper.Map<ProductComment, ProductCommentDto>(productComment);
        }

        [Authorize(BackOfficePermissions.ProductComments.Edit)]
        public virtual async Task<ProductCommentDto> UpdateAsync(Guid id, ProductCommentUpdateDto input)
        {
            if (input.ProductId == default)
            {
                throw new UserFriendlyException(L["The {0} field is required.", L["Product"]]);
            }

            var productComment = await _productCommentManager.UpdateAsync(
            id,
            input.ProductId, input.Name, input.Content, input.Likes, input.CreatedAt
            );

            return ObjectMapper.Map<ProductComment, ProductCommentDto>(productComment);
        }
    }
}