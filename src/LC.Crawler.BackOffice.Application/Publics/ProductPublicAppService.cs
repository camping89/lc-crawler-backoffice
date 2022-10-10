using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LC.Crawler.BackOffice.Categories;
using LC.Crawler.BackOffice.DataSources;
using LC.Crawler.BackOffice.Medias;
using LC.Crawler.BackOffice.ProductComments;
using LC.Crawler.BackOffice.ProductReviews;
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
    private readonly IProductCommentRepository _productCommentRepository;
    private readonly IProductReviewRepository _productReviewRepository;

    public ProductPublicAppService(IProductRepository productRepository,
        ProductManager productManager,
        IRepository<Media, Guid> mediaRepository,
        IRepository<DataSource, Guid> dataSourceRepository,
        IRepository<Category, Guid> categoryRepository,
        IProductCommentRepository productCommentRepository,
        IProductReviewRepository productReviewRepository)
    {
        _productRepository = productRepository;
        _productManager = productManager;
        _mediaRepository = mediaRepository;
        _dataSourceRepository = dataSourceRepository;
        _categoryRepository = categoryRepository;
        _productCommentRepository = productCommentRepository;
        _productReviewRepository = productReviewRepository;
    }

    public virtual async Task<PagedResultDto<ProductWithNavigationPropertiesResultDto>> GetListAsync(GetProductsInput input)
    {
        var totalCount = await _productRepository.GetCountAsync(input.FilterText,
            input.Name,
            input.Code,
            input.ShortDescription,
            input.Description,
            input.ExternalIdMin, 
            input.ExternalIdMax,
            input.FeaturedMediaId,
            input.DataSourceId,
            input.CategoryId,
            input.MediaId);
        var items = await _productRepository.GetListWithNavigationPropertiesAsync(input.FilterText,
            input.Name,
            input.Code,
            input.ShortDescription,
            input.Description,
            input.ExternalIdMin, 
            input.ExternalIdMax,
            input.FeaturedMediaId,
            input.DataSourceId,
            input.CategoryId,
            input.MediaId,
            input.Sorting,
            input.MaxResultCount,
            input.SkipCount);

        var results = new List<ProductWithNavigationPropertiesResultDto>();
        foreach (var item in items)
        {
            var resultItem = new ProductWithNavigationPropertiesResultDto();
            resultItem.DataSource = item.DataSource.Url;
            ObjectMapper.Map(item.Product, resultItem.Product);
            ObjectMapper.Map(item.Medias, resultItem.Images);
            ObjectMapper.Map(item.Categories, resultItem.Categories);
            ObjectMapper.Map(item.Variants, resultItem.Variants);
            ObjectMapper.Map(item.Attributes, resultItem.Attributes);

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

    public async Task<ProductCommentsResultDto> GetProductCommentsAsync(Guid productId)
    {
        var comments = await _productCommentRepository.GetListAsync(x => x.ProductId == productId);
        var productNav = await _productRepository.GetWithNavigationPropertiesAsync(productId);
        return new ProductCommentsResultDto()
        {
            DataSource = productNav.DataSource.Url,
            Product = ObjectMapper.Map<Product, ProductResultDto>(productNav.Product),
            Comments = ObjectMapper.Map<List<ProductComment>, List<ProductCommentResultDto>>(comments)
        };
    }

    public async Task<ProductReviewsResultDto> GetProductReviewsAsync(Guid productId)
    {
        var reviews = await _productReviewRepository.GetListAsync(x => x.ProductId == productId);
        var productNav = await _productRepository.GetWithNavigationPropertiesAsync(productId);
        return new ProductReviewsResultDto()
        {
            DataSource = productNav.DataSource.Url,
            Product = ObjectMapper.Map<Product, ProductResultDto>(productNav.Product),
            Reviews = ObjectMapper.Map<List<ProductReview>, List<ProductReviewResultDto>>(reviews)
        };
    }
}