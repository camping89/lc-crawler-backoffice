using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LC.Crawler.BackOffice.Categories;
using LC.Crawler.BackOffice.Core;
using LC.Crawler.BackOffice.DataSources;
using LC.Crawler.BackOffice.Helpers;
using LC.Crawler.BackOffice.Medias;
using LC.Crawler.BackOffice.Payloads;
using LC.Crawler.BackOffice.ProductAttributes;
using LC.Crawler.BackOffice.ProductVariants;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace LC.Crawler.BackOffice.Products;

public class ProductManagerLongChau : DomainService
{
    private readonly IProductLongChauRepository _productLongChauRepository;
    private readonly ICategoryLongChauRepository _categoryLongChauRepository;
    private readonly IMediaLongChauRepository _mediaLongChauRepository;
    private readonly IProductVariantLongChauRepository _productVariantLongChauRepository;
    private readonly IProductAttributeLongChauRepository _productAttributeLongChauRepository;
    private readonly IDataSourceRepository _dataSourceRepository;

    public ProductManagerLongChau(IProductLongChauRepository productLongChauRepository, ICategoryLongChauRepository categoryLongChauRepository, IMediaLongChauRepository mediaLongChauRepository, IDataSourceRepository dataSourceRepository, IProductVariantLongChauRepository productVariantLongChauRepository,
        IProductAttributeLongChauRepository productAttributeLongChauRepository)
    {
        _productLongChauRepository = productLongChauRepository;
        _categoryLongChauRepository = categoryLongChauRepository;
        _mediaLongChauRepository = mediaLongChauRepository;
        _dataSourceRepository = dataSourceRepository;
        _productVariantLongChauRepository = productVariantLongChauRepository;
        _productAttributeLongChauRepository = productAttributeLongChauRepository;
    }

    public async Task ProcessingDataAsync(CrawlEcommercePayload ecommercePayload)
    {
        var dataSource = await _dataSourceRepository.GetAsync(x => x.Url.Contains(PageDataSourceConsts.LongChauUrl));
        if (dataSource == null)
        {
            return;
        }

        var rawCategories = ecommercePayload.Products.Select(x => x.Category).ToList();
        //await CreateCategoriesAsync(rawCategories);
        
        var categories = await _categoryLongChauRepository.GetListAsync();
        foreach (var rawProduct in ecommercePayload.Products)
        {
            var productExist = await _productLongChauRepository.FirstOrDefaultAsync(x => x.Code == rawProduct.Code);
            if (productExist != null)
            {
                continue;
            }
            var product = new Product(GuidGenerator.Create())
            {
                Name = rawProduct.Title,
                Code = rawProduct.Code,
                Description = rawProduct.Description,
                ShortDescription = rawProduct.ShortDescription,
                DataSourceId = dataSource.Id,
                Brand = rawProduct.Brand
            };
           
            var category = categories.FirstOrDefault(x => x.Name == rawProduct.Category);
            if (category == null)
            {
                category = new Category()
                {
                    Name = rawProduct.Category
                };
                await _categoryLongChauRepository.InsertAsync(category, true);
                categories.Add(category);
            }
            product.AddCategory(category.Id);

            if (rawProduct.ImageUrls != null)
            {
                var medias = await CreateMediasAsync(rawProduct.ImageUrls);
                foreach (var media in medias)
                {
                    product.AddMedia(media.Id);
                }
            }
            
            if (!string.IsNullOrEmpty(rawProduct.Description))
            {
                var mediaUrls = rawProduct.Description.GetImageUrls();

                if (mediaUrls.Any())
                {
                    var medias = mediaUrls.Select(url => new Media()
                    {
                        Url = url,
                        IsDowloaded = false
                    }).ToList();
                    await _mediaLongChauRepository.InsertManyAsync(medias, true);

                    product.Description = StringHtmlHelper.ReplaceImageUrls(rawProduct.Description, medias);
                        
                    foreach (var media in medias)
                    {
                        product.AddMedia(media.Id);
                    }
                }
            }
            //Variants
            if (rawProduct.Variants != null)
            {
                foreach (var variant in rawProduct.Variants)
                {
                    await _productVariantLongChauRepository.InsertAsync(new ProductVariant()
                    {
                        ProductId = product.Id,
                        SKU = variant.SKU,
                        DiscountedPrice = variant.DiscountedPrice,
                        DiscountRate = variant.DiscountRate,
                        RetailPrice = variant.RetailPrice
                    },true);
                }
            }
            
            //Attributes
            if (rawProduct.Attributes != null)
            {
                foreach (var attribute in rawProduct.Attributes)
                {
                    await _productAttributeLongChauRepository.InsertAsync(new ProductAttribute()
                    {
                        Key = attribute.Key,
                        Slug = attribute.Slug,
                        Value = attribute.Value,
                        ProductId = product.Id
                    },true);
                }
            }

            await _productLongChauRepository.InsertAsync(product, true);
        }
    }

    private async Task<List<Media>> CreateMediasAsync(List<string> urls)
    {
        var medias = urls.Select(x => new Media()
        {
            Url = x,
            IsDowloaded = false
        }).ToList();

        await _mediaLongChauRepository.InsertManyAsync(medias, true);
        return medias;
    }

    // private async Task CreateCategoriesAsync(List<string> categoriesStr)
    // {
    //     categoriesStr = categoriesStr.Distinct().ToList();
    //     foreach (var categoryStr in categoriesStr)
    //     {
    //         var categoryItems = categoryStr.Split(">").ToList();
    //         foreach (var categoryItem in categoryItems)
    //         {
    //             var cateExist = await _categoryLongChauRepository.FirstOrDefaultAsync(x => x.Name.Equals(categoryItem, StringComparison.InvariantCultureIgnoreCase));
    //         }
    //     }
    // }
}