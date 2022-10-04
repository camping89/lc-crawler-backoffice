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
using LC.Crawler.BackOffice.ProductAttributes;

namespace LC.Crawler.BackOffice.ProductAttributes
{

    [Authorize(BackOfficePermissions.ProductAttributes.Default)]
    public class ProductAttributesAppService : ApplicationService, IProductAttributesAppService
    {
        private readonly IProductAttributeRepository _productAttributeRepository;
        private readonly ProductAttributeManager _productAttributeManager;
        private readonly IRepository<Product, Guid> _productRepository;

        public ProductAttributesAppService(IProductAttributeRepository productAttributeRepository, ProductAttributeManager productAttributeManager, IRepository<Product, Guid> productRepository)
        {
            _productAttributeRepository = productAttributeRepository;
            _productAttributeManager = productAttributeManager; _productRepository = productRepository;
        }

        public virtual async Task<PagedResultDto<ProductAttributeWithNavigationPropertiesDto>> GetListAsync(GetProductAttributesInput input)
        {
            var totalCount = await _productAttributeRepository.GetCountAsync(input.FilterText, input.Slug, input.Key, input.Value, input.ProductId);
            var items = await _productAttributeRepository.GetListWithNavigationPropertiesAsync(input.FilterText, input.Slug, input.Key, input.Value, input.ProductId, input.Sorting, input.MaxResultCount, input.SkipCount);

            return new PagedResultDto<ProductAttributeWithNavigationPropertiesDto>
            {
                TotalCount = totalCount,
                Items = ObjectMapper.Map<List<ProductAttributeWithNavigationProperties>, List<ProductAttributeWithNavigationPropertiesDto>>(items)
            };
        }

        public virtual async Task<ProductAttributeWithNavigationPropertiesDto> GetWithNavigationPropertiesAsync(Guid id)
        {
            return ObjectMapper.Map<ProductAttributeWithNavigationProperties, ProductAttributeWithNavigationPropertiesDto>
                (await _productAttributeRepository.GetWithNavigationPropertiesAsync(id));
        }

        public virtual async Task<ProductAttributeDto> GetAsync(Guid id)
        {
            return ObjectMapper.Map<ProductAttribute, ProductAttributeDto>(await _productAttributeRepository.GetAsync(id));
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

        [Authorize(BackOfficePermissions.ProductAttributes.Delete)]
        public virtual async Task DeleteAsync(Guid id)
        {
            await _productAttributeRepository.DeleteAsync(id);
        }

        [Authorize(BackOfficePermissions.ProductAttributes.Create)]
        public virtual async Task<ProductAttributeDto> CreateAsync(ProductAttributeCreateDto input)
        {
            if (input.ProductId == default)
            {
                throw new UserFriendlyException(L["The {0} field is required.", L["Product"]]);
            }

            var productAttribute = await _productAttributeManager.CreateAsync(
            input.ProductId, input.Slug, input.Key, input.Value
            );

            return ObjectMapper.Map<ProductAttribute, ProductAttributeDto>(productAttribute);
        }

        [Authorize(BackOfficePermissions.ProductAttributes.Edit)]
        public virtual async Task<ProductAttributeDto> UpdateAsync(Guid id, ProductAttributeUpdateDto input)
        {
            if (input.ProductId == default)
            {
                throw new UserFriendlyException(L["The {0} field is required.", L["Product"]]);
            }

            var productAttribute = await _productAttributeManager.UpdateAsync(
            id,
            input.ProductId, input.Slug, input.Key, input.Value
            );

            return ObjectMapper.Map<ProductAttribute, ProductAttributeDto>(productAttribute);
        }
    }
}