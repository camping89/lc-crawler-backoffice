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
using LC.Crawler.BackOffice.ProductVariants;

namespace LC.Crawler.BackOffice.ProductVariants
{

    [Authorize(BackOfficePermissions.ProductVariants.Default)]
    public class ProductVariantsAppService : ApplicationService, IProductVariantsAppService
    {
        private readonly IProductVariantRepository _productVariantRepository;
        private readonly ProductVariantManager _productVariantManager;
        private readonly IRepository<Product, Guid> _productRepository;

        public ProductVariantsAppService(IProductVariantRepository productVariantRepository, ProductVariantManager productVariantManager, IRepository<Product, Guid> productRepository)
        {
            _productVariantRepository = productVariantRepository;
            _productVariantManager = productVariantManager; _productRepository = productRepository;
        }

        public virtual async Task<PagedResultDto<ProductVariantWithNavigationPropertiesDto>> GetListAsync(GetProductVariantsInput input)
        {
            var totalCount = await _productVariantRepository.GetCountAsync(input.FilterText, input.SKU, input.RetailPriceMin, input.RetailPriceMax, input.DiscountRateMin, input.DiscountRateMax, input.DiscountedPriceMin, input.DiscountedPriceMax, input.ProductId);
            var items = await _productVariantRepository.GetListWithNavigationPropertiesAsync(input.FilterText, input.SKU, input.RetailPriceMin, input.RetailPriceMax, input.DiscountRateMin, input.DiscountRateMax, input.DiscountedPriceMin, input.DiscountedPriceMax, input.ProductId, input.Sorting, input.MaxResultCount, input.SkipCount);

            return new PagedResultDto<ProductVariantWithNavigationPropertiesDto>
            {
                TotalCount = totalCount,
                Items = ObjectMapper.Map<List<ProductVariantWithNavigationProperties>, List<ProductVariantWithNavigationPropertiesDto>>(items)
            };
        }

        public virtual async Task<ProductVariantWithNavigationPropertiesDto> GetWithNavigationPropertiesAsync(Guid id)
        {
            return ObjectMapper.Map<ProductVariantWithNavigationProperties, ProductVariantWithNavigationPropertiesDto>
                (await _productVariantRepository.GetWithNavigationPropertiesAsync(id));
        }

        public virtual async Task<ProductVariantDto> GetAsync(Guid id)
        {
            return ObjectMapper.Map<ProductVariant, ProductVariantDto>(await _productVariantRepository.GetAsync(id));
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

        [Authorize(BackOfficePermissions.ProductVariants.Delete)]
        public virtual async Task DeleteAsync(Guid id)
        {
            await _productVariantRepository.DeleteAsync(id);
        }

        [Authorize(BackOfficePermissions.ProductVariants.Create)]
        public virtual async Task<ProductVariantDto> CreateAsync(ProductVariantCreateDto input)
        {
            if (input.ProductId == default)
            {
                throw new UserFriendlyException(L["The {0} field is required.", L["Product"]]);
            }

            var productVariant = await _productVariantManager.CreateAsync(
            input.ProductId, input.SKU, input.RetailPrice, input.DiscountRate, input.DiscountedPrice
            );

            return ObjectMapper.Map<ProductVariant, ProductVariantDto>(productVariant);
        }

        [Authorize(BackOfficePermissions.ProductVariants.Edit)]
        public virtual async Task<ProductVariantDto> UpdateAsync(Guid id, ProductVariantUpdateDto input)
        {
            if (input.ProductId == default)
            {
                throw new UserFriendlyException(L["The {0} field is required.", L["Product"]]);
            }

            var productVariant = await _productVariantManager.UpdateAsync(
            id,
            input.ProductId, input.SKU, input.RetailPrice, input.DiscountRate, input.DiscountedPrice
            );

            return ObjectMapper.Map<ProductVariant, ProductVariantDto>(productVariant);
        }
    }
}