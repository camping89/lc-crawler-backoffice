﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LC.Crawler.BackOffice.Core;
using LC.Crawler.BackOffice.DataSources;
using LC.Crawler.BackOffice.Extensions;
using LC.Crawler.BackOffice.Helpers;
using LC.Crawler.BackOffice.Medias;
using LC.Crawler.BackOffice.Products;
using Volo.Abp.Domain.Services;
using WooCommerceNET;
using WooCommerceNET.WooCommerce.v3;
using WordPressPCL;
using WordPressPCL.Models;
using WooProductCategory = WooCommerceNET.WooCommerce.v3.ProductCategory;
using WooProductAttribute = WooCommerceNET.WooCommerce.v3.ProductAttribute;
using WooProduct = WooCommerceNET.WooCommerce.v3.Product;
using Category = LC.Crawler.BackOffice.Categories.Category;

namespace LC.Crawler.BackOffice.WooCommerces;

public class WooManangerBase : DomainService
{
    public async Task<List<ProductImage>> PostMediasAsync(DataSource dataSource, List<Media> medias)
    {
        if (medias == null)
        {
            return null;
        }

        //pass the Wordpress REST API base address as string
        var client = new WordPressClient($"{dataSource.PostToSite}/wp-json/");
        client.Auth.UseBasicAuth(dataSource.Configuration.Username, dataSource.Configuration.Password);
        var mediaItems = new List<MediaItem>();
        foreach (var media in medias.Where(media => !StringExtensions.IsNullOrEmpty(media.Url)))
        {
            //var stream = await _mediaManagerLongChau.GetFileStream(media.Name);
            if (media.Url.Contains("http") == false)
            {
                media.Url = $"{dataSource.Url}{media.Url}";
            }

            var fileExtension = Path.GetExtension(media.Url);
            var fileBytes = await FileExtendHelper.DownloadFile(media.Url);
            if (fileBytes != null && !string.IsNullOrEmpty(fileExtension))
            {
                var stream = new MemoryStream(fileBytes);
                var fileName = $"{media.Id}{fileExtension}";
                var mediaResult = await client.Media.CreateAsync(stream, fileName, media.ContentType);
                media.ExternalId = mediaResult.Id.ToString();
                media.ExternalUrl = mediaResult.SourceUrl;
                mediaItems.Add(mediaResult);
            }
        }

        return mediaItems.Select(x => new ProductImage { src = x.SourceUrl }).ToList();
    }

    public async Task<List<WooProductCategory>> GetWooCategories(DataSource dataSource)
    {
        var rest = new RestAPI($"{dataSource.PostToSite}/wp-json/wc/v3/", dataSource.Configuration.ApiKey, dataSource.Configuration.ApiSecret);
        var wcObject = new WCObject(rest);
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

        return wooCategories;
    }


    public async Task SyncCategoriesAsync(DataSource dataSource, List<Category> categories, string display = "products")
    {
        var rest = new RestAPI($"{dataSource.PostToSite}/wp-json/wc/v3/", dataSource.Configuration.ApiKey, dataSource.Configuration.ApiSecret);
        var wcObject = new WCObject(rest);

        var categoryNames = categories.Select(x => x.Name).Distinct().ToList();
        var wooCategories = await GetWooCategories(dataSource);
        foreach (var cateStr in categoryNames)
        {
            if (string.IsNullOrEmpty(cateStr))
            {
                continue;
            }

            var categoriesTerms = cateStr.Split("->").ToList();

            var cateName = categoriesTerms.FirstOrDefault()?.Trim().Replace("&", "&amp;");
            var wooRootCategory = wooCategories.FirstOrDefault(x => x.name.Equals(cateName, StringComparison.InvariantCultureIgnoreCase));
            if (wooRootCategory == null)
            {
                var cateNew = new WooProductCategory
                {
                    name = cateName,
                    display = display
                };
                wooRootCategory = await wcObject.Category.Add(cateNew);
                wooCategories.Add(wooRootCategory);
            }

            if (categoriesTerms.Count > 1 && wooRootCategory != null)
            {
                var cateParent = wooRootCategory;
                for (var i = 1; i < categoriesTerms.Count; i++)
                {
                    var subCateName = categoriesTerms[i].Trim().Replace("&", "&amp;");

                    var wooSubCategory = wooCategories.FirstOrDefault(x => x.name.Equals(subCateName, StringComparison.InvariantCultureIgnoreCase));
                    if (wooSubCategory == null)
                    {
                        var cateNew = new WooProductCategory
                        {
                            name = subCateName,
                            parent = cateParent.id,
                            display = display
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
            }
        }
    }

    public async Task<WooProduct> PostToWooProduct(DataSource dataSource, WCObject wcObject, ProductWithNavigationProperties productNav, List<WooProductCategory> wooCategories)
    {
        var checkProduct = await wcObject.Product.GetAll(new Dictionary<string, string>()
        {
            { "sku", productNav.Product.Code }
        });
        if (checkProduct.Count > 0)
        {
            return checkProduct.FirstOrDefault();
        }

        var product = productNav.Product;
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
            foreach (var category in productNav.Categories)
            {
                var encodeName = category.Name.Split("->").LastOrDefault()?.Replace("&", "&amp;").Trim();
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

        var variants = productNav.Variants;
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
            var medias = productNav.Medias;
            //Add product images
            if (medias != null)
            {
                wooProduct.images = new List<ProductImage>();
                wooProduct.images.AddRange(await PostMediasAsync(dataSource, medias));
            }

            wooProduct.description = StringHtmlHelper.ReplaceImageUrls(product.Description, medias);
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

        var attributes = productNav.Attributes;

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
}