using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Volo.Abp.Domain.Services;

using LC.Crawler.BackOffice.Categories;
using LC.Crawler.BackOffice.Core;
using LC.Crawler.BackOffice.DataSources;
using LC.Crawler.BackOffice.Medias;
using LC.Crawler.BackOffice.ProductAttributes;
using LC.Crawler.BackOffice.Products;
using LC.Crawler.BackOffice.ProductVariants;
using Microsoft.Extensions.Logging;
using Volo.Abp.BackgroundWorkers.Hangfire;
using Volo.Abp.Domain.Repositories;
using WooCommerceNET;
using WooCommerceNET.WooCommerce.v3;
using WordPressPCL;
using WordPressPCL.Models;
using WordPressPCL.Models.Exceptions;
using Product = LC.Crawler.BackOffice.Products.Product;
using WooProductCategory = WooCommerceNET.WooCommerce.v3.ProductCategory;
using WooProductAttribute = WooCommerceNET.WooCommerce.v3.ProductAttribute;
using WooProduct = WooCommerceNET.WooCommerce.v3.Product;
namespace LC.Crawler.BackOffice.WooCommerces;

public class WooManagerLongChau : DomainService
{
    private readonly ICategoryLongChauRepository _categoryLongChauRepository;
    private readonly IProductLongChauRepository _productRepository;
    private readonly IDataSourceRepository _dataSourceRepository;
    private readonly IMediaLongChauRepository _mediaLongChauRepository;
    private readonly IProductVariantLongChauRepository _productVariantLongChauRepository;
    private readonly IProductAttributeLongChauRepository _productAttributeLongChauRepository;
    private readonly MediaManagerLongChau _mediaManagerLongChau;
    
    private string BASEURL = string.Empty;
    
    public WooManagerLongChau(IProductLongChauRepository productRepository,
        IDataSourceRepository dataSourceRepository,
        MediaManagerLongChau mediaManagerLongChau,
        IMediaLongChauRepository mediaLongChauRepository,
        IProductVariantLongChauRepository productVariantLongChauRepository,
        IProductAttributeLongChauRepository productAttributeLongChauRepository,
        ICategoryLongChauRepository categoryLongChauRepository)
    {
        _productRepository = productRepository;
        _dataSourceRepository = dataSourceRepository;
        _mediaManagerLongChau = mediaManagerLongChau;
        _mediaLongChauRepository = mediaLongChauRepository;
        _productVariantLongChauRepository = productVariantLongChauRepository;
        _productAttributeLongChauRepository = productAttributeLongChauRepository;
        _categoryLongChauRepository = categoryLongChauRepository;

    }

    public async Task DoSyncProductToWooAsync()
    {
        var dataSource = await _dataSourceRepository.GetAsync(x => x.Url.Contains(PageDataSourceConsts.LongChauUrl));
        if (dataSource == null)
        {
            return;
        }
        BASEURL = dataSource.PostToSite;
        
        var rest = new RestAPI($"{BASEURL}/wp-json/wc/v3/", "ck_8136a9a5c6e69f3357fe2df61f2efdbbf6818c9a", "cs_9e02d8609876a9e5ddf93930e8aeda07b6cdd76d");
        var wc = new WCObject(rest);

        var products = await _productRepository.GetListAsync(x=> x.DataSourceId == dataSource.Id && x.ExternalId == null);

       var number = 1;
        foreach (var product in products)
        {
            try
            {
                var wooProduct = await PostToWooProduct(wc, product);
                if (wooProduct is { id: > 0 })
                {
                    product.ExternalId = wooProduct.id.To<int>();
                    await _productRepository.UpdateAsync(product, true);
                    Debug.WriteLine($"Product -> {number}");
                    number++;
                }
            }
            catch (Exception e)
            {
                Logger.Log(LogLevel.Error, e, e.StackTrace);
            }
        }
    }

    public async Task DoSyncCategoriesAsync()
    {
        var dataSource = await _dataSourceRepository.GetAsync(x => x.Url.Contains(PageDataSourceConsts.LongChauUrl));
        if (dataSource == null)
        {
            return;
        }
        BASEURL = dataSource.PostToSite;
        
        var rest = new RestAPI($"{BASEURL}/wp-json/wc/v3/", "ck_8136a9a5c6e69f3357fe2df61f2efdbbf6818c9a", "cs_9e02d8609876a9e5ddf93930e8aeda07b6cdd76d");
        var wcObject = new WCObject(rest);

        var categories = (await _categoryLongChauRepository.GetListAsync()).Select(x=>x.Name).Distinct().ToList();
        //Category
        var wooCategories = await wcObject.Category.GetAll(new Dictionary<string, string>()
        {
            { "per_page", "100"}
        });
        foreach (var cateStr in categories)
        {
            var categoriesTerms = cateStr.Split("->").ToList();
           
            var cateName = categoriesTerms.FirstOrDefault();
            var wooRootCategory = wooCategories.FirstOrDefault(x => x.name.Equals(cateName, StringComparison.InvariantCultureIgnoreCase));
            if (wooRootCategory == null)
            {
                var cateNew = new WooProductCategory
                {
                    name = cateName
                };
                cateNew = await wcObject.Category.Add(cateNew);
                wooCategories.Add(cateNew);
            }
            
            if (categoriesTerms.Count > 1 && wooRootCategory != null)
            {
                var cateParent = wooRootCategory;
                for (var i = 1; i < categoriesTerms.Count; i++)
                {
                    var subCateName = categoriesTerms[i];
                    
                    var wooSubCategory = wooCategories.FirstOrDefault(x => x.name.Equals(subCateName, StringComparison.InvariantCultureIgnoreCase));
                    if (wooSubCategory == null)
                    {
                        var cateNew = new WooProductCategory
                        {
                            name = cateName,
                            parent = cateParent.id
                        };
                        
                        cateNew = await wcObject.Category.Add(cateNew);
                        wooCategories.Add(cateNew);

                        cateParent = cateNew;
                    }
                }
            }
        }
    }

    private async Task<WooProduct> PostToWooProduct(WCObject wcObject, Product product)
    {
        var wooProduct = new WooProduct()
        {
            name = product.Name,
            sku = product.Code,
            short_description = product.ShortDescription,
            enable_html_short_description = "true",
            description = product.Description,
            enable_html_description = true,
            attributes = new List<ProductAttributeLine>(),
            variations = new List<int>(),
            categories = new List<ProductCategoryLine>()
        };

        if (product.Categories != null)
        {
            var categoryIds = product.Categories.Select(x => x.CategoryId).ToList();
            var categoriesLongChau = await _categoryLongChauRepository.GetListAsync(x=>categoryIds.Contains(x.Id));
            //Category
            var wooCategories = await wcObject.Category.GetAll(new Dictionary<string, string>()
            {
                { "per_page", "100"}
            });
            
            foreach (var categoryLongChau in categoriesLongChau)
            {
                var encodeName = categoryLongChau.Name.Split("->").LastOrDefault().Replace("&","&amp;");
                var wooCategory = wooCategories.FirstOrDefault(x => x.name.Contains(encodeName, StringComparison.InvariantCultureIgnoreCase));
                if (wooCategory != null)
                {
                    wooProduct.categories.Add(new ProductCategoryLine()
                    {
                        id = wooCategory.id,
                        name = wooCategory.name,
                        slug = wooCategory.slug
                    });
                }
                else
                {
                    
                    wooProduct.categories.Add(new ProductCategoryLine()
                    {
                        id = wooCategory.id,
                        name = wooCategory.name,
                        slug = wooCategory.slug
                    });
                }
            }
        }
        
        var variants = await _productVariantLongChauRepository.GetListAsync(x => x.ProductId == product.Id);
        if (variants != null)
        {
            decimal? productPrice = variants.Count > 1 ? null : variants.FirstOrDefault()?.RetailPrice;
            decimal? discountedPrice = variants.Count > 1 ? null : variants.FirstOrDefault()?.DiscountedPrice;

            if (productPrice.HasValue && productPrice > 0)
            {
                wooProduct.price = productPrice;
                wooProduct.regular_price = productPrice;
            }
            if (discountedPrice.HasValue && discountedPrice > 0)
            {
                wooProduct.sale_price = discountedPrice;
            }
        }

        if (product.Medias != null)
        {
            var mediaIds = product.Medias.Select(x => x.MediaId);
            var medias = await _mediaLongChauRepository.GetListAsync(x => mediaIds.Contains(x.Id));
            //Add product images
            if (medias != null)
            {
                wooProduct.images = new List<ProductImage>();
                wooProduct.images.AddRange(await PostMediasAsync(medias));
            }
            
        }
        
        //Variations
        if (variants is { Count: > 1 })
        {
            foreach (var variant in variants)
            {
                var wooVariantResult = await wcObject.Product.Variations.Add(new Variation()
                {
                    price = variant.RetailPrice,
                    regular_price = variant.RetailPrice,
                    sale_price = variant.DiscountedPrice
                }, 0);
                if (wooVariantResult.id is > 0)
                {
                    wooProduct.variations.Add(wooVariantResult.id.To<int>());
                }
            }
        }

        var attributes = await _productAttributeLongChauRepository.GetListAsync(x => x.ProductId == product.Id);

        //Attributes 
        if (attributes != null)
        {
            foreach (var attribute in attributes)
            {
                wooProduct.attributes.Add(new ProductAttributeLine()
                {
                    name = attribute.Key,
                    visible = true,
                    options = new List<string>(){ attribute.Value}
                });  
            }
        }
        
        return  await wcObject.Product.Add(wooProduct);
    }
    
    private async Task<List<ProductImage>> PostMediasAsync(List<Media> medias)
    {
        if (medias == null)
        {
            return null;
        }
        //pass the Wordpress REST API base address as string
        var client = new WordPressClient($"{BASEURL}/wp-json/");
        client.Auth.UseBasicAuth("admin", "123456");
        var mediaItems = new List<MediaItem>();
        foreach (var media in medias)
        {
            //var stream = await _mediaManagerLongChau.GetFileStream(media.Name);
            
            var fileBytes = await FileExtendHelper.DownloadFile(media.Url);
            if (fileBytes != null)
            {
                var stream = new MemoryStream(fileBytes);
                var fileName = media.Url.Split("/").LastOrDefault();
                var mediaResult = await client.Media.CreateAsync(stream, fileName, media.ContentType);
            
                media.ExternalId = mediaResult.Id.ToString();
                media.ExternalUrl = mediaResult.SourceUrl;
                await _mediaLongChauRepository.UpdateAsync(media, true);
            
                mediaItems.Add(mediaResult);
            }
        }
        
        return mediaItems.Select(x=> new ProductImage{src = x.SourceUrl }).ToList();
    }
}