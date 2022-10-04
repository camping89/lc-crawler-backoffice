using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LC.Crawler.BackOffice.Categories;
using LC.Crawler.BackOffice.DataSources;
using LC.Crawler.BackOffice.Medias;
using LC.Crawler.BackOffice.Products;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace LC.Crawler.BackOffice.Publics;

[RemoteService(IsEnabled = false)]
public class ProductPublicAppService : ApplicationService, IProductPublicAppService
{
    private readonly IProductRepository _productRepository;
    private readonly ProductManager _productManager;
    private readonly IRepository<Media, Guid> _mediaRepository;
    private readonly IRepository<DataSource, Guid> _dataSourceRepository;
    private readonly IRepository<Category, Guid> _categoryRepository;

    public ProductPublicAppService(IProductRepository productRepository, ProductManager productManager, IRepository<Media, Guid> mediaRepository, IRepository<DataSource, Guid> dataSourceRepository, IRepository<Category, Guid> categoryRepository)
    {
        _productRepository = productRepository;
        _productManager = productManager;
        _mediaRepository = mediaRepository;
        _dataSourceRepository = dataSourceRepository;
        _categoryRepository = categoryRepository;
            
    }

    public virtual async Task<PagedResultDto<ProductWithNavigationPropertiesResultDto>> GetListAsync(GetProductsInput input)
    {
        var totalCount = await _productRepository.GetCountAsync(input.FilterText, input.Name, input.Code, input.ShortDescription, input.Description, input.FeaturedMediaId, input.DataSourceId, input.CategoryId, input.MediaId);
        var items = await _productRepository.GetListWithNavigationPropertiesAsync(input.FilterText, input.Name, input.Code, input.ShortDescription, input.Description, input.FeaturedMediaId, input.DataSourceId, input.CategoryId, input.MediaId, input.Sorting, input.MaxResultCount, input.SkipCount);

        var results = new List<ProductWithNavigationPropertiesResultDto>();
        foreach (var item in items)
        {
            var resultItem = new ProductWithNavigationPropertiesResultDto();
            resultItem.DataSource = item.DataSource.Url;
            ObjectMapper.Map(item.Product,resultItem.Product);
            ObjectMapper.Map(item.Medias,resultItem.Images);
            ObjectMapper.Map(item.Categories,resultItem.Categories);
            if (resultItem.Product != null)
            {
                resultItem.Product.FeatureImageUrl = item.Media.Url;
            }
            results.Add(resultItem);
        }
        return new PagedResultDto<ProductWithNavigationPropertiesResultDto>
        {
            TotalCount = totalCount,
            Items = results
        };
    }

}