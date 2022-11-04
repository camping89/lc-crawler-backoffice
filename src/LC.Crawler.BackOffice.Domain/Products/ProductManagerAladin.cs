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

public class ProductManagerAladin : DomainService
{
    private readonly IProductAladinRepository _productAladinRepository;
    private readonly ICategoryAladinRepository _categoryAladinRepository;
    private readonly IMediaAladinRepository _mediaAladinRepository;
    private readonly IProductAttributeAladinRepository _productAttributeAladinRepository;
    private readonly IProductVariantAladinRepository _productVariantAladinRepository;
    private readonly IDataSourceRepository _dataSourceRepository;
    private readonly ITrackingDataSourceRepository _trackingDataSourceRepository;

    public ProductManagerAladin(IProductAladinRepository productAladinRepository, ICategoryAladinRepository categoryAladinRepository, IMediaAladinRepository mediaAladinRepository, IDataSourceRepository dataSourceRepository, IProductAttributeAladinRepository productAttributeAladinRepository,
        IProductVariantAladinRepository productVariantAladinRepository, ITrackingDataSourceRepository trackingDataSourceRepository)
    {
        _productAladinRepository = productAladinRepository;
        _categoryAladinRepository = categoryAladinRepository;
        _mediaAladinRepository = mediaAladinRepository;
        _dataSourceRepository = dataSourceRepository;
        _productAttributeAladinRepository = productAttributeAladinRepository;
        _productVariantAladinRepository = productVariantAladinRepository;
        _trackingDataSourceRepository = trackingDataSourceRepository;
    }

    public async  Task ProcessingDataAsync(CrawlEcommercePayload ecommercePayload)
    {
        var dataSource = await _dataSourceRepository.GetAsync(x => x.Url.Contains(PageDataSourceConsts.AladinUrl));
        if (dataSource == null)
        {
            return;
        }
        var categories = await _categoryAladinRepository.GetListAsync();
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
            
            var productExist = await _productAladinRepository.FirstOrDefaultAsync(x => x.Code == rawProduct.Code);
            if (productExist != null)
            {
                var attributes =
                    await _productAttributeAladinRepository.GetListAsync(_ => _.ProductId == productExist.Id);

                //Init new attribute from raw product not in db
                foreach (var rawAttribute in from rawAttribute in rawProduct.Attributes
                         let attribute =
                             attributes.Where(_ => _.Key == rawAttribute.Key && _.Value == rawAttribute.Value)
                         where !attribute.IsNotNullOrEmpty()
                         select rawAttribute)
                {
                    await _productAttributeAladinRepository.InsertAsync(new ProductAttribute()
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
                    await _productAttributeAladinRepository.DeleteAsync(attribute);
                }

                productExist.Brand = rawProduct.Brand;
                productExist.Tags = rawProduct.Tags;
                await _productAladinRepository.UpdateAsync(productExist, true);
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
                    await _categoryAladinRepository.InsertAsync(category, true);
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
                    await _mediaAladinRepository.InsertManyAsync(medias, true);

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
                    await _productVariantAladinRepository.InsertAsync(new ProductVariant()
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
                    await _productAttributeAladinRepository.InsertAsync(new ProductAttribute()
                    {
                        Key = attribute.Key,
                        Slug = attribute.Slug,
                        Value = attribute.Value,
                        ProductId = product.Id
                    },true);
                }
            }

            await _productAladinRepository.InsertAsync(product, true);

        }
    }

    private async Task<List<Media>> CreateMediasAsync(List<string> urls)
    {
        var medias = urls.Select(x => new Media()
        {
            Url = x,
            IsDowloaded = false
        }).ToList();

        await _mediaAladinRepository.InsertManyAsync(medias,true);
        return medias;
    }
}