using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using Product = LC.Crawler.BackOffice.Products.Product;
using WooProductCategory = WooCommerceNET.WooCommerce.v3.ProductCategory;
using WooProductAttribute = WooCommerceNET.WooCommerce.v3.ProductAttribute;
using WooProduct = WooCommerceNET.WooCommerce.v3.Product;

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

    public WooManagerSieuThiSongKhoe(IProductSieuThiSongKhoeRepository productRepository,
        IDataSourceRepository dataSourceRepository,
        MediaManagerSieuThiSongKhoe mediaManagerSieuThiSongKhoe,
        IMediaSieuThiSongKhoeRepository mediaSieuThiSongKhoeRepository,
        IProductVariantSieuThiSongKhoeRepository productVariantSieuThiSongKhoeRepository,
        IProductAttributeSieuThiSongKhoeRepository productAttributeSieuThiSongKhoeRepository,
        ICategorySieuThiSongKhoeRepository categorySieuThiSongKhoeRepository)
    {
        _productRepository = productRepository;
        _dataSourceRepository = dataSourceRepository;
        _mediaManagerSieuThiSongKhoe = mediaManagerSieuThiSongKhoe;
        _mediaSieuThiSongKhoeRepository = mediaSieuThiSongKhoeRepository;
        _productVariantSieuThiSongKhoeRepository = productVariantSieuThiSongKhoeRepository;
        _productAttributeSieuThiSongKhoeRepository = productAttributeSieuThiSongKhoeRepository;
        _categorySieuThiSongKhoeRepository = categorySieuThiSongKhoeRepository;
    }

    public async Task DoSyncProductToWooAsync()
    {
        _dataSource = await _dataSourceRepository.GetAsync(x => x.Url.Contains(PageDataSourceConsts.SieuThiSongKhoeUrl));
        if (_dataSource == null)
        {
            return;
        }

        BASEURL = _dataSource.PostToSite;

        var rest = new RestAPI($"{BASEURL}/wp-json/wc/v3/", _dataSource.Configuration.ApiKey, _dataSource.Configuration.ApiSecret);
        var wc = new WCObject(rest);

        var products = await _productRepository.GetListAsync(x => x.DataSourceId == _dataSource.Id && x.ExternalId == null);

        var number = 1;
        foreach (var product in products.Take(10))
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
        _dataSource = await _dataSourceRepository.GetAsync(x => x.Url.Contains(PageDataSourceConsts.SieuThiSongKhoeUrl));
        if (_dataSource == null)
        {
            return;
        }

        BASEURL = _dataSource.PostToSite;

        var rest = new RestAPI($"{BASEURL}/wp-json/wc/v3/", _dataSource.Configuration.ApiKey,  _dataSource.Configuration.ApiSecret);
        var wcObject = new WCObject(rest);

        var categories = (await _categorySieuThiSongKhoeRepository.GetListAsync()).Select(x => x.Name).Distinct().ToList();
        //Category
        var wooCategories = new List<WooProductCategory>();
        var pageIndex = 1;
        while (true)
        {
            var wooCategoriesResult = await wcObject.Category.GetAll(new Dictionary<string, string>()
            {
                { "page", pageIndex.ToString() },
                { "per_page", "100" },
            });

            if (wooCategoriesResult.IsNullOrEmpty())
            {
                break;
            }

            wooCategories.AddRange(wooCategoriesResult);

            pageIndex++;
        }

        foreach (var cateStr in categories)
        {
            if (cateStr.IsNullOrEmpty())
            {
                continue;
            }

            var categoriesTerms = cateStr.Split("->").ToList();

            var cateName = categoriesTerms.FirstOrDefault()?.Trim().Replace("&", "&amp;");
            var wooRootCategory = wooCategories.FirstOrDefault(x => x.name.Equals(cateName, StringComparison.InvariantCultureIgnoreCase));
            if (wooRootCategory == null)
            {
                try
                {
                    var cateNew = new WooProductCategory
                    {
                        name = cateName,
                        display = "products"
                    };
                    wooRootCategory = await wcObject.Category.Add(cateNew);
                    wooCategories.Add(wooRootCategory);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }

            if (categoriesTerms.Count > 1 && wooRootCategory != null)
            {
                var cateParent = wooRootCategory;
                for (var i = 1; i < categoriesTerms.Count; i++)
                {
                    try
                    {
                        var subCateName = categoriesTerms[i].Trim().Replace("&", "&amp;");

                        var wooSubCategory = wooCategories.FirstOrDefault(x => x.name.Equals(subCateName, StringComparison.InvariantCultureIgnoreCase));
                        if (wooSubCategory == null)
                        {
                            var cateNew = new WooProductCategory
                            {
                                name = subCateName,
                                parent = cateParent.id,
                                display = "products"
                            };

                            cateNew = await wcObject.Category.Add(cateNew);
                            wooCategories.Add(cateNew);

                            cateParent = cateNew;
                        }
                        else
                        {
                            cateParent = wooSubCategory;
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
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
            categories = new List<ProductCategoryLine>(),
            status = "pending"
        };

        if (product.Categories != null)
        {
            var categoryIds = product.Categories.Select(x => x.CategoryId).ToList();
            var categoriesSieuThiSongKhoe = await _categorySieuThiSongKhoeRepository.GetListAsync(x => categoryIds.Contains(x.Id));
            //Category
            var wooCategories = new List<WooProductCategory>();
            var pageIndex = 1;
            while (true)
            {
                var wooCategoriesResult = await wcObject.Category.GetAll(new Dictionary<string, string>()
                {
                    { "page", pageIndex.ToString() },
                    { "per_page", "100" },
                });

                if (wooCategoriesResult.IsNullOrEmpty())
                {
                    break;
                }

                wooCategories.AddRange(wooCategoriesResult);

                pageIndex++;
            }

            foreach (var categorySieuThiSongKhoe in categoriesSieuThiSongKhoe)
            {
                var encodeName = categorySieuThiSongKhoe.Name.Split("->").LastOrDefault()?.Replace("&", "&amp;").Trim();
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
            }
        }

        var variants = await _productVariantSieuThiSongKhoeRepository.GetListAsync(x => x.ProductId == product.Id);
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
                    },
                    0);
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
                    options = new List<string>() { attribute.Value }
                });
            }
        }

        return await wcObject.Product.Add(wooProduct);
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
            if (media.Url.Contains("http") == false)
            {
                continue;
            }

            var fileBytes = await FileExtendHelper.DownloadFile(media.Url);
            if (fileBytes != null)
            {
                var stream = new MemoryStream(fileBytes);
                var fileName = media.Url.Split("/").LastOrDefault();
                var mediaResult = await client.Media.CreateAsync(stream, fileName, media.ContentType);

                media.ExternalId = mediaResult.Id.ToString();
                media.ExternalUrl = mediaResult.SourceUrl;
                await _mediaSieuThiSongKhoeRepository.UpdateAsync(media, true);

                mediaItems.Add(mediaResult);
            }
        }

        return mediaItems.Select(x => new ProductImage { src = x.SourceUrl }).ToList();
    }
}