using LC.Crawler.BackOffice.Shared;
using LC.Crawler.BackOffice.Medias;
using LC.Crawler.BackOffice.Categories;
using LC.Crawler.BackOffice.DataSources;
using LC.Crawler.BackOffice.Medias;
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
        private readonly IRepository<Media, Guid> _mediaRepository;
        private readonly IRepository<DataSource, Guid> _dataSourceRepository;
        private readonly IRepository<Category, Guid> _categoryRepository;

        public ProductsAppService(IProductRepository productRepository, ProductManager productManager, IRepository<Media, Guid> mediaRepository, IRepository<DataSource, Guid> dataSourceRepository, IRepository<Category, Guid> categoryRepository)
        {
            _productRepository = productRepository;
            _productManager = productManager;
            _mediaRepository = mediaRepository;
            _dataSourceRepository = dataSourceRepository;
            _categoryRepository = categoryRepository;
        }

        public virtual async Task<PagedResultDto<ProductWithNavigationPropertiesDto>> GetListAsync(GetProductsInput input)
        {
            var totalCount = await _productRepository.GetCountAsync(input.FilterText, input.Name, input.Code, input.ShortDescription, input.Description, input.ExternalIdMin, input.ExternalIdMax, input.FeaturedMediaId, input.DataSourceId, input.CategoryId, input.MediaId);
            var items = await _productRepository.GetListWithNavigationPropertiesAsync(input.FilterText, input.Name, input.Code, input.ShortDescription, input.Description, input.ExternalIdMin, input.ExternalIdMax, input.FeaturedMediaId, input.DataSourceId, input.CategoryId, input.MediaId, input.Sorting, input.MaxResultCount, input.SkipCount);

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

        public virtual async Task<PagedResultDto<LookupDto<Guid?>>> GetMediaLookupAsync(LookupRequestDto input)
        {
            var query = (await _mediaRepository.GetQueryableAsync())
                .WhereIf(!string.IsNullOrWhiteSpace(input.Filter),
                    x => x.Url != null &&
                         x.Url.Contains(input.Filter));

            var lookupData = await query.PageBy(input.SkipCount, input.MaxResultCount).ToDynamicListAsync<Media>();
            var totalCount = query.Count();
            return new PagedResultDto<LookupDto<Guid?>>
            {
                TotalCount = totalCount,
                Items = ObjectMapper.Map<List<Media>, List<LookupDto<Guid?>>>(lookupData)
            };
        }

        public virtual async Task<PagedResultDto<LookupDto<Guid>>> GetDataSourceLookupAsync(LookupRequestDto input)
        {
            var query = (await _dataSourceRepository.GetQueryableAsync())
                .WhereIf(!string.IsNullOrWhiteSpace(input.Filter),
                    x => x.Url != null &&
                         x.Url.Contains(input.Filter));

            var lookupData = await query.PageBy(input.SkipCount, input.MaxResultCount).ToDynamicListAsync<DataSource>();
            var totalCount = query.Count();
            return new PagedResultDto<LookupDto<Guid>>
            {
                TotalCount = totalCount,
                Items = ObjectMapper.Map<List<DataSource>, List<LookupDto<Guid>>>(lookupData)
            };
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

        [Authorize(BackOfficePermissions.Products.Delete)]
        public virtual async Task DeleteAsync(Guid id)
        {
            await _productRepository.DeleteAsync(id);
        }

        [Authorize(BackOfficePermissions.Products.Create)]
        public virtual async Task<ProductDto> CreateAsync(ProductCreateDto input)
        {
            if (input.DataSourceId == default)
            {
                throw new UserFriendlyException(L["The {0} field is required.", L["DataSource"]]);
            }

            var product = await _productManager.CreateAsync(
            input.CategoryIds, input.MediaIds, input.FeaturedMediaId, input.DataSourceId, input.Name, input.Code, input.ShortDescription, input.Description, input.ExternalId
            );

            return ObjectMapper.Map<Product, ProductDto>(product);
        }

        [Authorize(BackOfficePermissions.Products.Edit)]
        public virtual async Task<ProductDto> UpdateAsync(Guid id, ProductUpdateDto input)
        {
            if (input.DataSourceId == default)
            {
                throw new UserFriendlyException(L["The {0} field is required.", L["DataSource"]]);
            }

            var product = await _productManager.UpdateAsync(
            id,
            input.CategoryIds, input.MediaIds, input.FeaturedMediaId, input.DataSourceId, input.Name, input.Code, input.ShortDescription, input.Description, input.ExternalId, input.ConcurrencyStamp
            );

            return ObjectMapper.Map<Product, ProductDto>(product);
        }
    }
}