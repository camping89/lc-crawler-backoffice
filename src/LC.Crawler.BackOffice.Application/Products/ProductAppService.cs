using LC.Crawler.BackOffice.Shared;
using LC.Crawler.BackOffice.Medias;
using LC.Crawler.BackOffice.Categories;
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
using LC.Crawler.BackOffice.Products;

namespace LC.Crawler.BackOffice.Products
{

    [Authorize(BackOfficePermissions.Products.Default)]
    public class ProductsAppService : ApplicationService, IProductsAppService
    {
        private readonly IProductRepository _productRepository;
        private readonly ProductManager _productManager;
        private readonly IRepository<Category, Guid> _categoryRepository;
        private readonly IRepository<Media, Guid> _mediaRepository;

        public ProductsAppService(IProductRepository productRepository, ProductManager productManager, IRepository<Category, Guid> categoryRepository, IRepository<Media, Guid> mediaRepository)
        {
            _productRepository = productRepository;
            _productManager = productManager; _categoryRepository = categoryRepository;
            _mediaRepository = mediaRepository;
        }

        public virtual async Task<PagedResultDto<ProductWithNavigationPropertiesDto>> GetListAsync(GetProductsInput input)
        {
            var totalCount = await _productRepository.GetCountAsync(input.FilterText, input.Name, input.Brand, input.RatingMin, input.RatingMax, input.PriceMin, input.PriceMax, input.DiscountPercentMin, input.DiscountPercentMax, input.ShortDescription, input.Description, input.CategoryId, input.MediaId);
            var items = await _productRepository.GetListWithNavigationPropertiesAsync(input.FilterText, input.Name, input.Brand, input.RatingMin, input.RatingMax, input.PriceMin, input.PriceMax, input.DiscountPercentMin, input.DiscountPercentMax, input.ShortDescription, input.Description, input.CategoryId, input.MediaId, input.Sorting, input.MaxResultCount, input.SkipCount);

            return new PagedResultDto<ProductWithNavigationPropertiesDto>
            {
                TotalCount = totalCount,
                Items = ObjectMapper.Map<List<ProductWithNavigationProperties>, List<ProductWithNavigationPropertiesDto>>(items)
            };
        }

        public virtual async Task<ProductWithNavigationPropertiesDto> GetWithNavigationPropertiesAsync(Guid id)
        {
            return ObjectMapper.Map<ProductWithNavigationProperties, ProductWithNavigationPropertiesDto>
                (await _productRepository.GetWithNavigationPropertiesAsync(id));
        }

        public virtual async Task<ProductDto> GetAsync(Guid id)
        {
            return ObjectMapper.Map<Product, ProductDto>(await _productRepository.GetAsync(id));
        }

        public virtual async Task<PagedResultDto<LookupDto<Guid>>> GetCategoryLookupAsync(LookupRequestDto input)
        {
            var query = (await _categoryRepository.GetQueryableAsync())
                .WhereIf(!string.IsNullOrWhiteSpace(input.Filter),
                    x => x.Name != null &&
                         x.Name.Contains(input.Filter));

            var lookupData = await query.PageBy(input.SkipCount, input.MaxResultCount).ToDynamicListAsync<Category>();
            var totalCount = query.Count();
            return new PagedResultDto<LookupDto<Guid>>
            {
                TotalCount = totalCount,
                Items = ObjectMapper.Map<List<Category>, List<LookupDto<Guid>>>(lookupData)
            };
        }

        public virtual async Task<PagedResultDto<LookupDto<Guid>>> GetMediaLookupAsync(LookupRequestDto input)
        {
            var query = (await _mediaRepository.GetQueryableAsync())
                .WhereIf(!string.IsNullOrWhiteSpace(input.Filter),
                    x => x.Url != null &&
                         x.Url.Contains(input.Filter));

            var lookupData = await query.PageBy(input.SkipCount, input.MaxResultCount).ToDynamicListAsync<Media>();
            var totalCount = query.Count();
            return new PagedResultDto<LookupDto<Guid>>
            {
                TotalCount = totalCount,
                Items = ObjectMapper.Map<List<Media>, List<LookupDto<Guid>>>(lookupData)
            };
        }

        [Authorize(BackOfficePermissions.Products.Delete)]
        public virtual async Task DeleteAsync(Guid id)
        {
            await _productRepository.DeleteAsync(id);
        }

        [Authorize(BackOfficePermissions.Products.Create)]
        public virtual async Task<ProductDto> CreateAsync(ProductCreateDto input)
        {

            var product = await _productManager.CreateAsync(
            input.CategoryIds, input.MediaIds, input.Name, input.Brand, input.Rating, input.Price, input.DiscountPercent, input.ShortDescription, input.Description
            );

            return ObjectMapper.Map<Product, ProductDto>(product);
        }

        [Authorize(BackOfficePermissions.Products.Edit)]
        public virtual async Task<ProductDto> UpdateAsync(Guid id, ProductUpdateDto input)
        {

            var product = await _productManager.UpdateAsync(
            id,
            input.CategoryIds, input.MediaIds, input.Name, input.Brand, input.Rating, input.Price, input.DiscountPercent, input.ShortDescription, input.Description, input.ConcurrencyStamp
            );

            return ObjectMapper.Map<Product, ProductDto>(product);
        }
    }
}