using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LC.Crawler.BackOffice.Categories;
using LC.Crawler.BackOffice.Core;
using LC.Crawler.BackOffice.DataSources;
using LC.Crawler.BackOffice.Medias;
using LC.Crawler.BackOffice.ProductAttributes;
using LC.Crawler.BackOffice.Products;
using LC.Crawler.BackOffice.ProductVariants;
using Microsoft.Extensions.Logging;
using Volo.Abp.Domain.Services;
using WooCommerceNET;
using WooCommerceNET.WooCommerce.v3;
using WordPressPCL;
using WordPressPCL.Models;
using Product = WooCommerceNET.WooCommerce.v3.Product;
using ProductCategory = WooCommerceNET.WooCommerce.v3.ProductCategory;
namespace LC.Crawler.BackOffice.WooCommerces;

public class WooManagerSieuThiSongKhoe : DomainService
{
    private readonly ICategorySieuThiSongKhoeRepository _categorySieuThiSongKhoeRepository;
    private readonly IProductSieuThiSongKhoeRepository _productRepository;
    private readonly IDataSourceRepository _dataSourceRepository;
    private readonly IMediaSieuThiSongKhoeRepository _mediaSieuThiSongKhoeRepository;
    private readonly IProductVariantSieuThiSongKhoeRepository _productVariantSieuThiSongKhoeRepository;
    private readonly IProductAttributeSieuThiSongKhoeRepository _productAttributeSieuThiSongKhoeRepository;
    private readonly MediaManagerSieuThiSongKhoe _mediaManagerSieuThiSongKhoe;
    
    private string BASEURL = string.Empty;
    private DataSource _dataSource;

    public WooManagerSieuThiSongKhoe(ICategorySieuThiSongKhoeRepository categorySieuThiSongKhoeRepository, IProductSieuThiSongKhoeRepository productRepository, IDataSourceRepository dataSourceRepository, IMediaSieuThiSongKhoeRepository mediaSieuThiSongKhoeRepository, IProductVariantSieuThiSongKhoeRepository productVariantSieuThiSongKhoeRepository, IProductAttributeSieuThiSongKhoeRepository productAttributeSieuThiSongKhoeRepository, MediaManagerSieuThiSongKhoe mediaManagerSieuThiSongKhoe)
    {
        _categorySieuThiSongKhoeRepository = categorySieuThiSongKhoeRepository;
        _productRepository = productRepository;
        _dataSourceRepository = dataSourceRepository;
        _mediaSieuThiSongKhoeRepository = mediaSieuThiSongKhoeRepository;
        _productVariantSieuThiSongKhoeRepository = productVariantSieuThiSongKhoeRepository;
        _productAttributeSieuThiSongKhoeRepository = productAttributeSieuThiSongKhoeRepository;
        _mediaManagerSieuThiSongKhoe = mediaManagerSieuThiSongKhoe;
    }
    
    public async Task DoSyncProductToWooAsync()
    {
        var dataSource = await _dataSourceRepository.GetAsync(x => x.Url.Contains(PageDataSourceConsts.SieuThiSongKhoeUrl));
        if (dataSource == null)
        {
            return;
        }

        _dataSource = dataSource;
        BASEURL = dataSource.PostToSite;
        
        var rest = new RestAPI($"{BASEURL}/wp-json/wc/v3/", _dataSource.Configuration.ApiKey, _dataSource.Configuration.ApiSecret);
        var wc = new WCObject(rest);

        var products = await _productRepository.GetListAsync(x=> x.DataSourceId == dataSource.Id && x.ExternalId == null);

        foreach (var product in products)
        {
            try
            {
                var wooProduct = await PostToWooProduct(wc, product);
                if (wooProduct is { id: > 0 })
                {
                    product.ExternalId = wooProduct.id.To<int>();
                    await _productRepository.UpdateAsync(product, true);
                }
            }
            catch (Exception e)
            {
                Logger.Log(LogLevel.Error, e, e.StackTrace);
            }
        }
    }

    private async Task<Product> PostToWooProduct(WCObject wcObject, Products.Product product)
    {
        var wooProduct = new Product()
        {
            name = product.Name,
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
            var categoriesSieuThiSongKhoe = await _categorySieuThiSongKhoeRepository.GetListAsync(x=>categoryIds.Contains(x.Id));
            //Category
            var wooCategories = await wcObject.Category.GetAll();
            
            foreach (var categorySieuThiSongKhoe in categoriesSieuThiSongKhoe)
            {
                var wooCategory = wooCategories.FirstOrDefault(x => x.name.Equals(categorySieuThiSongKhoe.Name));
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
                    wooCategory = await wcObject.Category.Add(new ProductCategory
                    {
                        name = categorySieuThiSongKhoe.Name,
                        slug = categorySieuThiSongKhoe.Slug
                    });
                    
                    wooProduct.categories.Add(new ProductCategoryLine()
                    {
                        id = wooCategory.id,
                        name = wooCategory.name,
                        slug = wooCategory.slug
                    });
                }
            }
        }
        
        var variants = await _productVariantSieuThiSongKhoeRepository.GetListAsync(x => x.ProductId == product.Id);
        if (variants != null)
        {
            decimal? productPrice = variants.Count > 1 ? null : variants.FirstOrDefault()?.RetailPrice;
            decimal? discountedPrice = variants.Count > 1 ? null : variants.FirstOrDefault()?.DiscountedPrice;
            
            wooProduct.price = productPrice;
            wooProduct.regular_price = productPrice;
            wooProduct.sale_price = discountedPrice;
        }

        if (product.Medias != null)
        {
            var mediaIds = product.Medias.Select(x => x.MediaId);
            var medias = await _mediaSieuThiSongKhoeRepository.GetListAsync(x => mediaIds.Contains(x.Id));
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

        var attributes = await _productAttributeSieuThiSongKhoeRepository.GetListAsync(x => x.ProductId == product.Id);

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
        client.Auth.UseBasicAuth(_dataSource.Configuration.Username, _dataSource.Configuration.Password);
        var mediaItems = new List<MediaItem>();
        foreach (var media in medias)
        {
            //var stream = await _mediaManagerSieuThiSongKhoe.GetFileStream(media.Name);
            
            var fileBytes = await FileExtendHelper.DownloadFile(media.Url);
            if (fileBytes != null)
            {
                var stream = new MemoryStream(fileBytes);
                var mediaResult = await client.Media.CreateAsync(stream, media.Name, media.ContentType);
            
                media.ExternalId = mediaResult.Id.ToString();
                media.ExternalUrl = mediaResult.SourceUrl;
                await _mediaSieuThiSongKhoeRepository.UpdateAsync(media, true);
            
                mediaItems.Add(mediaResult);
            }
        }
        
        return mediaItems.Select(x=> new ProductImage{src = x.SourceUrl }).ToList();
    }
}