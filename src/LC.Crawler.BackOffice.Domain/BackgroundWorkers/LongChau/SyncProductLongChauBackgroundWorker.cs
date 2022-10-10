using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using LC.Crawler.BackOffice.Articles;
using LC.Crawler.BackOffice.Medias;
using LC.Crawler.BackOffice.Products;
using Volo.Abp.BackgroundWorkers.Hangfire;
using WooCommerceNET;
using WooCommerceNET.WooCommerce.v3;
using WooCommerceNET.WooCommerce.v3.Extension;
using WordPressPCL;
using WordPressPCL.Models;
using WooProduct = WooCommerceNET.WooCommerce.v3.Product;

namespace LC.Crawler.BackOffice.BackgroundWorkers.LongChau;

public class SyncProductLongChauBackgroundWorker : HangfireBackgroundWorkerBase
{
    private readonly IProductLongChauRepository _productLongChauRepository;
    
    private readonly MediaManagerLongChau _mediaManagerLongChau;
    private readonly string BASEURL = "https://103.116.104.43";

    public SyncProductLongChauBackgroundWorker(IProductLongChauRepository productLongChauRepository, MediaManagerLongChau mediaManagerLongChau)
    {
        _productLongChauRepository = productLongChauRepository;
        _mediaManagerLongChau = mediaManagerLongChau;


        RecurringJobId            = nameof(SyncProductLongChauBackgroundWorker);
        CronExpression            = Cron.Daily(0,0);
    }

    public override async Task DoWorkAsync()
    {
        var rest = new RestAPI($"{BASEURL}/wp-json/wc/v3/", "<WooCommerce Key>", "<WooCommerce Secret");
        var wc = new WCObject(rest);
        
        var products = await _productLongChauRepository.GetListWithNavigationPropertiesAsync();
        foreach (var productNav in products)
        {
            var wooProduct = await PostToWooProduct(wc, productNav);
            if (wooProduct is { id: > 0 })
            {
                productNav.Product.ExternalId = wooProduct.id.To<int>();
                await _productLongChauRepository.UpdateAsync(productNav.Product);
            }
        }
    }
    
    private async Task<WooProduct> PostToWooProduct(WCObject wcObject, ProductWithNavigationProperties productWithNav)
    {
        var product = productWithNav.Product;
        decimal? productPrice = productWithNav.Variants.Count > 0 ? null : productWithNav.Variants.FirstOrDefault()?.RetailPrice;
        var wooProduct = new WooProduct()
        {
            name = product.Name,
            short_description = product.ShortDescription,
            enable_html_short_description = "true",
            description = product.Description,
            enable_html_description = true,
            price = productPrice,
        };
        
        //Add product images
        wooProduct.images.AddRange(await PostMediasAsync(productWithNav));
        
        await wcObject.Product.Add(wooProduct);
        
        //TODO: Support multiple variants
        if (wooProduct.id > 0)
        {
            //Variations
            if (productWithNav.Variants != null && productWithNav.Variants.Count > 2)
            {
                foreach (var variant in productWithNav.Variants)
                {
                    await wcObject.Product.Variations.Add(new Variation()
                    {
                        price = variant.RetailPrice,
                        regular_price = variant.RetailPrice,
                        sale_price = variant.DiscountedPrice
                    }, wooProduct.id.Value.To<int>());
                }
            }

            await wcObject.Product.Update(product.Id.To<int>(), wooProduct);
        }

        return wooProduct;
    }
    
    private async Task<List<ProductImage>> PostMediasAsync(ProductWithNavigationProperties productNav)
    {
        //pass the Wordpress REST API base address as string
        var client = new WordPressClient($"{BASEURL}/wp-json/");
        var mediaItems = new List<MediaItem>();
        foreach (var media in productNav.Medias)
        {
            var stream = await _mediaManagerLongChau.GetFileStream(media.Name);
            mediaItems.Add(await client.Media.CreateAsync(stream,media.Name, media.ContentType));
        }

        return mediaItems.Select(x=> new ProductImage{src = x.Link }).ToList();
    }
}