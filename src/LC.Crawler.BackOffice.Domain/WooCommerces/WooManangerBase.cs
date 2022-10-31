using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ExCSS;
using IdentityServer4.Extensions;
using LC.Crawler.BackOffice.Articles;
using LC.Crawler.BackOffice.Core;
using LC.Crawler.BackOffice.DataSources;
using LC.Crawler.BackOffice.Extensions;
using LC.Crawler.BackOffice.Helpers;
using LC.Crawler.BackOffice.Medias;
using LC.Crawler.BackOffice.Products;
using Volo.Abp.Auditing;
using Svg;
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
            if(media.ExternalId.IsNotNullOrEmpty()) continue;
            MediaItem mediaResult;
            //var stream = await _mediaManagerLongChau.GetFileStream(media.Name);
            if (!media.Url.Contains("http"))
            {
                media.Url = Flurl.Url.Combine(dataSource.Url, media.Url);
            }

            media.Url = HtmlExtendHelper.RemoveQueryStringByKey(media.Url);
            var fileExtension = Path.GetExtension(media.Url);
            if (!fileExtension.IsNotNullOrEmpty()) return null;

            if (fileExtension is FileExtendHelper.SvgExtend)
            {
                var svgContent = await FileExtendHelper.DownloadSvgFile(media.Url);
                if (!svgContent.IsNotNullOrEmpty()) return null;

                var fileName = $"{media.Id}{FileExtendHelper.PngExtend}";
                var svgDoc = SvgDocument.FromSvg<SvgDocument>(svgContent);
                var bitmap = svgDoc.Draw();
                using var stream = new MemoryStream();
                bitmap.Save(stream, ImageFormat.Png);
                stream.Position = 0;
                mediaResult = await client.Media.CreateAsync(stream, fileName, media.ContentType);
            }
            else
            {
                var fileBytes = await FileExtendHelper.DownloadFile(media.Url);
                if (fileBytes is null) return null;
                var stream = new MemoryStream(fileBytes);
                var fileName = $"{media.Id}{fileExtension}";
                mediaResult = await client.Media.CreateAsync(stream, fileName, media.ContentType);
            }

            media.ExternalId = mediaResult.Id.ToString();
            media.ExternalUrl = mediaResult.SourceUrl;
            mediaItems.Add(mediaResult);
        }

        return mediaItems.Select(x => new ProductImage { src = x.SourceUrl }).ToList();
    }

    public async Task<List<WooProductCategory>> GetWooCategories(DataSource dataSource)
    {
        var rest = new RestAPI($"{dataSource.PostToSite}/wp-json/wc/v3/", dataSource.Configuration.ApiKey,
            dataSource.Configuration.ApiSecret);
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

    public async Task<List<ProductTag>> GetWooProductTagsAsync(DataSource dataSource)
    {
        var rest = new RestAPI($"{dataSource.PostToSite}/wp-json/wc/v3/", dataSource.Configuration.ApiKey,
            dataSource.Configuration.ApiSecret);
        var wcObject = new WCObject(rest);
        //Category
        var wooTags = new List<ProductTag>();
        var pageIndex = 1;
        while (true)
        {
            var wooTagsResult = await wcObject.Tag.GetAll(new Dictionary<string, string>()
            {
                { "page", pageIndex.ToString() },
                { "per_page", "100" },
            });

            if (wooTagsResult.IsNullOrEmpty())
            {
                break;
            }

            wooTags.AddRange(wooTagsResult);

            pageIndex++;
        }

        return wooTags;
    }

    public async Task SyncProductTagsAsync(DataSource dataSource, List<string> tags)
    {
        var wooTags = await GetWooProductTagsAsync(dataSource);
        var productTagNeedCreate = tags.Where(x => !wooTags.Any(t => t.name.Equals(x, StringComparison.InvariantCultureIgnoreCase))).ToList();
        if (productTagNeedCreate.IsNotNullOrEmpty())
        {
            var rest = new RestAPI($"{dataSource.PostToSite}/wp-json/wc/v3/", dataSource.Configuration.ApiKey,
                                dataSource.Configuration.ApiSecret);
            var wcObject = new WCObject(rest);
            foreach (var tag in productTagNeedCreate)
            {
                await wcObject.Tag.Add(new ProductTag()
                {
                    name = tag
                });
            }
        }
    }

    public async Task SyncCategoriesAsync(DataSource dataSource, List<Category> categories, string display = "products")
    {
        var rest = new RestAPI($"{dataSource.PostToSite}/wp-json/wc/v3/", dataSource.Configuration.ApiKey,
            dataSource.Configuration.ApiSecret);
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
            var wooRootCategory =
                wooCategories.FirstOrDefault(x => x.name.Equals(cateName, StringComparison.InvariantCultureIgnoreCase));
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
                    
                    var wooSubCategory = wooCategories.FirstOrDefault(x =>
                        x.name.Equals(subCateName, StringComparison.InvariantCultureIgnoreCase) && x.parent == cateParent.id);
                    
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

    public async Task<WooProduct> PostToWooProduct(DataSource dataSource, WCObject wcObject,
        ProductWithNavigationProperties productNav, List<WooProductCategory> wooCategories,
        List<ProductTag> productTags)
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
            tags = new List<ProductTagLine>(),
            categories = new List<ProductCategoryLine>(),
            status = "pending"
        };

        if (product.Categories != null)
        {
            foreach (var category in productNav.Categories)
            {
                if (string.IsNullOrEmpty(category.Name))
                {
                    continue;
                }
                
                category.Name = category.Name.Replace("&", "&amp;").Trim();
                var cateTerms = category.Name.Split("->").LastOrDefault();
                //Thuốc -> Vitamin &amp; khoáng chất
                //Thực phẩm chức năng -> Vitamin &amp; khoáng chất
                var encodeName = cateTerms?.Replace("&", "&amp;").Trim();
                
                var wooCategory = wooCategories.FirstOrDefault(x =>
                    encodeName != null && x.name.Contains(encodeName, StringComparison.InvariantCultureIgnoreCase));
                if (encodeName != null && encodeName.Equals("Vitamin &amp; khoáng chất", StringComparison.InvariantCultureIgnoreCase))
                {
                    var wooCategoriesFilter = wooCategories.Where(x =>
                        x.name.Contains(encodeName, StringComparison.InvariantCultureIgnoreCase)).ToList();
                    foreach (var wooCate in wooCategoriesFilter)
                    {
                        var parentCate = wooCategories.FirstOrDefault(x => x.id == wooCate.parent);
                        if (parentCate != null && category.Name.Contains(parentCate.name))
                        {
                            wooCategory = wooCate;
                        }
                    }
                }

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

        //Tags 
        var tags = productNav.Product.Tags;
        if (tags.IsNotNullOrEmpty())
        {
            foreach (var tag in tags)
            {
                var productTag = productTags.FirstOrDefault(x => x.name.Contains(tag, StringComparison.InvariantCultureIgnoreCase));
                if (productTag is null)
                {
                    productTag = new ProductTag() { name = tag };
                    productTag = await wcObject.Tag.Add(productTag);
                }

                if (productTag.id > 0)
                {
                    wooProduct.tags.Add(new ProductTagLine()
                    {
                        id   = productTag.id,
                        name = productTag.name,
                        slug = productTag.slug
                    });
                }
            }
        }
        
        return await wcObject.Product.Add(wooProduct);
    }

    public void LogException(AuditLogInfo currentLog, Exception ex, ProductWithNavigationProperties productNav,
        string url)
    {
        //Add exceptions
        currentLog.Url = url;
        currentLog.Exceptions.Add(ex);
        if (ex.InnerException is not null)
        {
            currentLog.Exceptions.Add(ex.InnerException);
        }

        currentLog.Comments.Add($"Id: {productNav.Product.Id}, DataSourceId {productNav.Product.DataSourceId}");
        currentLog.Comments.Add(ex.StackTrace);
        currentLog.ExtraProperties.Add("C_Message", ex.Message);
        currentLog.ExtraProperties.Add("C_StackTrace", ex.StackTrace);
        currentLog.ExtraProperties.Add("C_Source", ex.Source);
        currentLog.ExtraProperties.Add("C_ExToString", ex.ToString());
    }

    public async Task DeleteDuplicateWooProduct(DataSource dataSource)
    {
        var rest = new RestAPI($"{dataSource.PostToSite}/wp-json/wc/v3/", dataSource.Configuration.ApiKey,
            dataSource.Configuration.ApiSecret);
        var wc = new WCObject(rest);

        var checkProducts = new List<WooCommerceNET.WooCommerce.v3.Product>();
        var pageIndex = 1;
        while (true)
        {
            var checkProduct = await wc.Product.GetAll(new Dictionary<string, string>()
            {
                { "page", pageIndex.ToString() },
                { "per_page", "100" },
            });
            if (checkProduct.IsNullOrEmpty()) break;

            checkProducts.AddRange(checkProduct);
            pageIndex++;
        }

        var deletedProducts = checkProducts.GroupBy(_ => _.sku).Where(_ => _.Count() > 1).ToList();
        foreach (var item in deletedProducts)
        {
            var deleteProduct = await wc.Product.Delete(item.FirstOrDefault().id.To<int>());
            Console.WriteLine($"-----------------------Deleted Product: {deleteProduct.id}-----------------------");
        }
    }
}