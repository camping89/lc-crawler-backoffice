using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Extensions;
using LC.Crawler.BackOffice.Categories;
using LC.Crawler.BackOffice.Core;
using LC.Crawler.BackOffice.DataSources;
using LC.Crawler.BackOffice.Enums;
using LC.Crawler.BackOffice.Extensions;
using LC.Crawler.BackOffice.Helpers;
using LC.Crawler.BackOffice.Medias;
using LC.Crawler.BackOffice.Payloads;
using LC.Crawler.BackOffice.ProductAttributes;
using LC.Crawler.BackOffice.ProductVariants;
using LC.Crawler.BackOffice.TrackingDataSources;
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
    private readonly ITrackingDataSourceRepository _trackingDataSourceRepository;

    public ProductManagerLongChau(IProductLongChauRepository productLongChauRepository,
        ICategoryLongChauRepository categoryLongChauRepository, IMediaLongChauRepository mediaLongChauRepository,
        IDataSourceRepository dataSourceRepository, IProductVariantLongChauRepository productVariantLongChauRepository,
        IProductAttributeLongChauRepository productAttributeLongChauRepository,
        ITrackingDataSourceRepository trackingDataSourceRepository)
    {
        _productLongChauRepository = productLongChauRepository;
        _categoryLongChauRepository = categoryLongChauRepository;
        _mediaLongChauRepository = mediaLongChauRepository;
        _dataSourceRepository = dataSourceRepository;
        _productVariantLongChauRepository = productVariantLongChauRepository;
        _productAttributeLongChauRepository = productAttributeLongChauRepository;
        _trackingDataSourceRepository = trackingDataSourceRepository;
    }

    public async Task ProcessingDataAsync(CrawlEcommercePayload ecommercePayload)
    {
        var dataSource = await _dataSourceRepository.GetAsync(x => x.Url.Contains(PageDataSourceConsts.LongChauUrl));
        if (dataSource == null)
        {
            return;
        }

        var categories = await _categoryLongChauRepository.GetListAsync();
        foreach (var rawProducts in ecommercePayload.Products.GroupBy(_ => _.Url))
        {
            var rawProduct = rawProducts.First();
            if (AbpStringExtensions.IsNullOrEmpty(rawProduct.Code))
            {
                await _trackingDataSourceRepository.InsertAsync(new TrackingDataSource()
                {
                    Url = rawProduct.Url,
                    CrawlType = CrawlType.Ecom,
                    PageDataSource = PageDataSource.LongChau,
                    Error = TrackingDataSourceConsts.EmptyCode
                }, true);
            }

            var productExist = await _productLongChauRepository.FirstOrDefaultAsync(x => x.Code == rawProduct.Code);
            if (productExist != null)
            {
                var attributes =
                    await _productAttributeLongChauRepository.GetListAsync(_ => _.ProductId == productExist.Id);

                //Init new attribute from raw product not in db
                foreach (var rawAttribute in from rawAttribute in rawProduct.Attributes
                         let attribute =
                             attributes.Where(_ => _.Key == rawAttribute.Key && _.Value == rawAttribute.Value)
                         where !attribute.IsNotNullOrEmpty()
                         select rawAttribute)
                {
                    await _productAttributeLongChauRepository.InsertAsync(new ProductAttribute()
                    {
                        Key = rawAttribute.Key,
                        Slug = rawAttribute.Slug,
                        Value = rawAttribute.Value,
                        ProductId = productExist.Id
                    }, true);
                }

                //Delete attribute from db not in raw product
                foreach (var attribute in from attribute in attributes
                         let rawAttribute =
                             rawProduct.Attributes.Where(_ => _.Key == attribute.Key && _.Value == attribute.Value)
                         where rawAttribute.IsNullOrEmpty()
                         select attribute)
                {
                    await _productAttributeLongChauRepository.DeleteAsync(attribute);
                }

                productExist.Brand = rawProduct.Brand;
                productExist.Tags = rawProduct.Tags;
                await _productLongChauRepository.UpdateAsync(productExist, true);
                continue;
            }

            var product = new Product(GuidGenerator.Create())
            {
                Name = rawProduct.Title,
                Code = rawProduct.Code,
                Description = rawProduct.Description,
                ShortDescription = rawProduct.ShortDescription,
                DataSourceId = dataSource.Id,
                Brand = rawProduct.Brand,
                Tags = rawProduct.Tags
            };

            foreach (var raw in rawProducts)
            {
                var category = categories.FirstOrDefault(x => x.Name == raw.Category);
                if (category == null)
                {
                    category = new Category()
                    {
                        Name = raw.Category
                    };
                    await _categoryLongChauRepository.InsertAsync(category, true);
                    categories.Add(category);
                }

                product.AddCategory(category.Id);
            }

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

                    product.Description = StringHtmlHelper.SetContentMediaIds(rawProduct.Description, medias);

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
                    }, true);
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
                    }, true);
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

        if (medias.Any())
        {
            await _mediaLongChauRepository.InsertManyAsync(medias, true);
        }

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

    public async Task UpdateData(CrawlEcommercePayload ecommercePayload)
    {
        var dataSource = await _dataSourceRepository.GetAsync(x => x.Url.Contains(PageDataSourceConsts.LongChauUrl));
        if (dataSource == null)
        {
            return;
        }

        var categories = await _categoryLongChauRepository.GetListAsync();
        foreach (var rawProducts in ecommercePayload.Products.GroupBy(_ => _.Url))
        {
            var rawProduct = rawProducts.First();

            var productExist = await _productLongChauRepository.FirstOrDefaultAsync(x => x.Code == rawProduct.Code);
            if (productExist == null)
            {
                continue;
            }
            
            foreach (var raw in rawProducts)
            {
                var category = categories.FirstOrDefault(x => x.Name == raw.Category);
                if (category == null)
                {
                    category = new Category()
                    {
                        Name = raw.Category
                    };
                    await _categoryLongChauRepository.InsertAsync(category, true);
                    categories.Add(category);
                }

                if(productExist.Categories.Select(_ => _.CategoryId).Contains(category.Id))
                {
                    productExist.AddCategory(category.Id);
                }
            }

            var attributes =
                await _productAttributeLongChauRepository.GetListAsync(_ => _.ProductId == productExist.Id);

            //Init new attribute from raw product not in db
            foreach (var rawAttribute in from rawAttribute in rawProduct.Attributes
                     let attribute =
                         attributes.Where(_ => _.Key == rawAttribute.Key && _.Value == rawAttribute.Value)
                     where !attribute.IsNotNullOrEmpty()
                     select rawAttribute)
            {
                await _productAttributeLongChauRepository.InsertAsync(new ProductAttribute()
                {
                    Key = rawAttribute.Key,
                    Slug = rawAttribute.Slug,
                    Value = rawAttribute.Value,
                    ProductId = productExist.Id
                }, true);
            }

            //Delete attribute from db not in raw product
            foreach (var attribute in from attribute in attributes
                     let rawAttribute =
                         rawProduct.Attributes.Where(_ => _.Key == attribute.Key && _.Value == attribute.Value)
                     where rawAttribute.IsNullOrEmpty()
                     select attribute)
            {
                await _productAttributeLongChauRepository.DeleteAsync(attribute);
            }

            productExist.Brand = rawProduct.Brand;
            productExist.Tags = rawProduct.Tags;
            await _productLongChauRepository.UpdateAsync(productExist, true);
        }
    }
}