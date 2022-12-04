using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LC.Crawler.BackOffice.Core;
using LC.Crawler.BackOffice.DataSources;
using LC.Crawler.BackOffice.Extensions;
using LC.Crawler.BackOffice.Helpers;
using LC.Crawler.BackOffice.Medias;
using LC.Crawler.BackOffice.ProductComments;
using LC.Crawler.BackOffice.Products;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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
using Product = LC.Crawler.BackOffice.Products.Product;
using ProductReview = LC.Crawler.BackOffice.ProductReviews.ProductReview;

namespace LC.Crawler.BackOffice.WooCommerces;

public class WooManangerBase : DomainService
{
    private readonly IAuditingManager _auditingManager;

    public WooManangerBase(IAuditingManager auditingManager)
    {
        _auditingManager = auditingManager;
    }

    public async Task<List<ProductImage>> PostMediasAsync(DataSource dataSource, List<Media> medias)
    {
        if (medias == null)
        {
            return null;
        }

        var productImages = new List<ProductImage>();
        //pass the Wordpress REST API base address as string
        var client = new WordPressClient($"{dataSource.PostToSite}/wp-json/");
        client.Auth.UseBasicAuth(dataSource.Configuration.Username, dataSource.Configuration.Password);
        var mediaItems = new List<MediaItem>();
        foreach (var media in medias.Where(media => !StringExtensions.IsNullOrEmpty(media.Url)))
        {
            if (media.ExternalId.IsNotNullOrEmpty()) continue;
            MediaItem mediaResult;
            //var stream = await _mediaManagerLongChau.GetFileStream(media.Name);
            if (!media.Url.Contains("http"))
            {
                media.Url = $"{dataSource.Url}{media.Url}";
            }

            media.Url = HtmlExtendHelper.RemoveQueryStringByKey(media.Url);
            var fileExtension = Path.GetExtension(media.Url);
            if (!fileExtension.IsNotNullOrEmpty()) continue;

            if (fileExtension.Equals(FileExtendHelper.SvgExtend, StringComparison.InvariantCultureIgnoreCase))
            {
                var svgContent = await FileExtendHelper.DownloadSvgFile(media.Url);
                if (!svgContent.IsNotNullOrEmpty()) continue;

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
                if (!fileExtension.Equals(FileExtendHelper.PngExtend, StringComparison.InvariantCultureIgnoreCase) && !fileExtension.Equals(FileExtendHelper.JpgExtend, StringComparison.InvariantCultureIgnoreCase))
                    fileExtension = FileExtendHelper.PngExtend;
                var fileBytes = await FileExtendHelper.DownloadFile(media.Url);
                if (fileBytes is null) continue;
                var stream = new MemoryStream(fileBytes);
                var fileName = $"{media.Id}{fileExtension}";
                mediaResult = await client.Media.CreateAsync(stream, fileName, media.ContentType);
            }

            media.ExternalId = mediaResult.Id.ToString();
            media.ExternalUrl = mediaResult.SourceUrl;
            var productImage = new ProductImage()
            {
                src = media.Url
            };
            if (mediaResult.SourceUrl.EndsWith(".webp"))
            {
                productImage.src = media.Url;
            }

            productImages.Add(productImage);
            mediaItems.Add(mediaResult);
        }

        //return mediaItems.Select(x => new ProductImage { src = x.SourceUrl }).ToList();
        return productImages;
    }

    public async Task<List<WooProductCategory>> GetWooCategories(DataSource dataSource)
    {
        var wcObject = await InitWCObject(dataSource);

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
        var wcObject = await InitWCObject(dataSource);
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
            var wcObject = await InitWCObject(dataSource);
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
        var wcObject = await InitWCObject(dataSource);

        var categoryNames = categories.Select(x => x.Name).Distinct().ToList();
        var wooCategories = await GetWooCategories(dataSource);
        foreach (var cateStr in categoryNames)
        {
            if (string.IsNullOrEmpty(cateStr))
            {
                continue;
            }

            var categoriesTerms = cateStr.Split("->").Select(x => x.Trim()).ToList();

            var cateName = categoriesTerms.FirstOrDefault()?.Trim().Replace("&", "&amp;");
            var wooRootCategory =
                wooCategories.FirstOrDefault(x => x.name.Equals(cateName, StringComparison.InvariantCultureIgnoreCase) && x.parent == 0);
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

    public async Task PostProductReviews(WCObject wcObject, string productCode, List<ProductComment> productComments, List<ProductReview> productReviews)
    {
        try
        {
            var checkProduct = await wcObject.Product.GetAll(new Dictionary<string, string>()
            {
                { "sku", productCode }
            });
            if (checkProduct.Count > 0)
            {
                var productUpdate = checkProduct.FirstOrDefault();
                if (productUpdate != null)
                {
                    if (productComments != null)
                    {
                        foreach (var productComment in productComments)
                        {
                            await wcObject.ProductReview.Add(new WooCommerceNET.WooCommerce.v3.ProductReview()
                            {
                                reviewer = productComment.Name,
                                review = productComment.Content,
                                product_id = productUpdate.id,
                                date_created = productComment.CreatedAt,
                                verified = true,
                                reviewer_email = $"reviewer@gmail.com"
                                //status = "approved"
                            });
                        }
                    }

                    if (productReviews != null)
                    {
                        foreach (var productReview in productReviews)
                        {
                            await wcObject.ProductReview.Add(new WooCommerceNET.WooCommerce.v3.ProductReview()
                            {
                                reviewer = productReview.Name,
                                review = productReview.Content,
                                product_id = productUpdate.id,
                                date_created = productReview.CreatedAt,
                                verified = true,
                                reviewer_email = $"reviewer@gmail.com"
                                //status = "approved"
                            });
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Logger.LogException(e);
        }
    }

    public async Task<WooProduct> PostToWooProduct(DataSource dataSource,
        WCObject wcObject,
        ProductWithNavigationProperties productNav,
        List<WooProductCategory> wooCategories,
        List<ProductTag> productTags)
    {
        var checkProduct = await wcObject.Product.GetAll(new Dictionary<string, string>()
        {
            { "sku", productNav.Product.Code },
        });
        if (checkProduct.Count > 0)
        {
            var productUpdate = checkProduct.FirstOrDefault();
            if (productUpdate != null)
            {
                // add variants
                await UpdateProductVariants(wcObject, productNav, productUpdate, dataSource.Url);
                await wcObject.Product.Update(productUpdate.id.To<int>(), productUpdate);
            }

            return productUpdate;
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

        // add categories
        await AddProductCategories(productNav, wooCategories, wooProduct, dataSource.Url);

        // add medias
        await AddProductMedias(dataSource, productNav, wooProduct);

        // add variants
        await AddProductVariants(wcObject, productNav, wooProduct, dataSource.Url);

        // add attributes
        await AddProductAttributes(productNav, wooProduct, dataSource.Url);

        // Add tags
        await AddProductTags(wcObject, productNav, productTags, wooProduct, dataSource.Url);

        var x = JsonConvert.SerializeObject(wooProduct);
        return await wcObject.Product.Add(wooProduct);
    }

    private async Task AddProductTags(WCObject wcObject,
        ProductWithNavigationProperties productNav,
        List<ProductTag> productTags,
        WooProduct wooProduct,
        string homeUrl)
    {
        using var auditingScope = _auditingManager.BeginScope();

        try
        {
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
                        wooProduct.tags.Add(new ProductTagLine() { id = productTag.id, name = productTag.name, slug = productTag.slug });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            LogException(_auditingManager.Current.Log, ex, productNav.Product, homeUrl, "ProductTags");
        }
        finally
        {
            //Always save the log
            await auditingScope.SaveAsync();
        }
    }

    private async Task AddProductAttributes(ProductWithNavigationProperties productNav, WooProduct wooProduct, string homeUrl)
    {
        using var auditingScope = _auditingManager.BeginScope();

        try
        {
            var attributes = productNav.Attributes;

            //Attributes 
            if (attributes != null)
            {
                foreach (var attribute in attributes)
                {
                    wooProduct.attributes.Add(new ProductAttributeLine() { name = attribute.Key, visible = true, options = new List<string>() { attribute.Value } });
                }
            }
        }
        catch (Exception ex)
        {
            LogException(_auditingManager.Current.Log, ex, productNav.Product, homeUrl, "ProductAttributes");
        }
        finally
        {
            //Always save the log
            await auditingScope.SaveAsync();
        }
    }

    private async Task AddProductVariants(WCObject wcObject, ProductWithNavigationProperties productNav, WooProduct wooProduct, string homeUrl)
    {
        using var auditingScope = _auditingManager.BeginScope();

        try
        {
            //Variations
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

            if (variants is { Count: > 1 })
            {
                foreach (var variant in variants)
                {
                    var wooVariantResult = await wcObject.Product.Variations.Add(new Variation()
                        {
                            sku = variant.SKU,
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
        }
        catch (Exception ex)
        {
            LogException(_auditingManager.Current.Log, ex, productNav.Product, homeUrl, "ProductVariants");
        }
        finally
        {
            //Always save the log
            await auditingScope.SaveAsync();
        }
    }


    private async Task UpdateProductVariants(WCObject wcObject, ProductWithNavigationProperties productNav, WooProduct wooProduct, string homeUrl)
    {
        using var auditingScope = _auditingManager.BeginScope();

        try
        {
            //Variations
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

            if (variants is { Count: > 1 })
            {
                var wooProductVariants = await wcObject.Product.Variations.GetAll(wooProduct.id);
                foreach (var variant in variants)
                {
                    var checkVariant = wooProductVariants.FirstOrDefault(x => x.sku == variant.SKU);
                    if (checkVariant != null)
                    {
                        checkVariant.price = variant.RetailPrice;
                        checkVariant.regular_price = variant.RetailPrice;
                        checkVariant.sale_price = variant.DiscountedPrice;
                        await wcObject.Product.Variations.Update(checkVariant.id.To<int>(), checkVariant, wooProduct.id.To<int>());
                    }
                    else
                    {
                        var wooVariantResult = await wcObject.Product.Variations.Add(new Variation()
                            {
                                sku = variant.SKU,
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
            }
        }
        catch (Exception ex)
        {
            LogException(_auditingManager.Current.Log, ex, productNav.Product, homeUrl, "ProductVariants");
        }
        finally
        {
            //Always save the log
            await auditingScope.SaveAsync();
        }
    }

    private async Task AddProductMedias(DataSource dataSource,
        ProductWithNavigationProperties productNav,
        WooProduct wooProduct)
    {
        using var auditingScope = _auditingManager.BeginScope();
        try
        {
            if (productNav.Medias != null)
            {
                var medias = productNav.Medias;

                //Add product images
                if (medias != null)
                {
                    wooProduct.images = new List<ProductImage>();
                    var mediaResults = await PostMediasAsync(dataSource, medias);
                    if (mediaResults.IsNotNullOrEmpty()) wooProduct.images.AddRange(mediaResults);
                }

                wooProduct.description = StringHtmlHelper.ReplaceImageUrls(productNav.Product.Description, medias);
            }
        }
        catch (Exception ex)
        {
            LogException(_auditingManager.Current.Log, ex, productNav.Product, dataSource.Url, "ProductMedias");
        }
        finally
        {
            //Always save the log
            await auditingScope.SaveAsync();
        }
    }

    private async Task AddProductCategories(ProductWithNavigationProperties productNav,
        List<WooProductCategory> wooCategories,
        WooProduct wooProduct,
        string homeUrl)
    {
        using var auditingScope = _auditingManager.BeginScope();
        try
        {
            if (productNav.Categories != null)
            {
                foreach (var category in productNav.Categories)
                {
                    if (string.IsNullOrEmpty(category.Name))
                    {
                        continue;
                    }

                    var cateTerms = category.Name.Split("->").LastOrDefault();

                    //Thuốc -> Vitamin &amp; khoáng chất
                    //Thực phẩm chức năng -> Vitamin &amp; khoáng chất
                    var encodeName = cateTerms?.Replace("&", "&amp;").Trim();

                    var wooCategory = wooCategories.FirstOrDefault(x => encodeName != null && x.name.Equals(encodeName, StringComparison.InvariantCultureIgnoreCase));
                    if (encodeName != null)
                    {
                        category.Name = category.Name.Replace("&", "&amp;").Trim();
                        var wooCategoriesFilter = wooCategories.Where(x => x.name.Equals(encodeName, StringComparison.InvariantCultureIgnoreCase)).ToList();
                        foreach (var wooCate in wooCategoriesFilter)
                        {
                            var parentCate = wooCategories.FirstOrDefault(x => x.id == wooCate.parent);
                            if (parentCate != null && category.Name.Contains(parentCate.name, StringComparison.InvariantCultureIgnoreCase))
                            {
                                var rootParent = wooCategories.FirstOrDefault(x => x.id == parentCate.parent);
                                if ((rootParent != null && category.Name.Contains(rootParent.name)) || parentCate.parent == 0)
                                {
                                    wooCategory = wooCate;
                                }
                            }
                        }
                    }

                    if (wooCategory != null)
                    {
                        wooProduct.categories.Add(new ProductCategoryLine() { id = wooCategory.id, name = wooCategory.name, slug = wooCategory.slug });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            LogException(_auditingManager.Current.Log, ex, productNav.Product, homeUrl, "ProductCategories");
        }
        finally
        {
            //Always save the log
            await auditingScope.SaveAsync();
        }
    }

    public void LogException(AuditLogInfo currentLog,
        Exception ex,
        Product product,
        string url,
        string entity = "Product")
    {
        //Add exceptions
        currentLog.Url = url;
        currentLog.Exceptions.Add(ex);
        if (ex.InnerException is not null)
        {
            currentLog.Exceptions.Add(ex.InnerException);
        }

        currentLog.Comments.Add($"Id: {product.Id}, DataSourceId {product.DataSourceId}");
        currentLog.Comments.Add(ex.StackTrace);
        currentLog.ExtraProperties.Add("C_Entity", entity);
        currentLog.ExtraProperties.Add("C_Message", ex.Message);
        currentLog.ExtraProperties.Add("C_StackTrace", ex.StackTrace);
        currentLog.ExtraProperties.Add("C_Source", ex.Source);
        currentLog.ExtraProperties.Add("C_ExToString", ex.ToString());
    }

    public async Task DeleteDuplicateWooProduct(DataSource dataSource)
    {
        var wc = await InitWCObject(dataSource);

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

    /// <summary>
    ///  Update the products are not found in the latest crawl
    /// </summary>
    /// <param name="dataSource"></param>
    /// <param name="products"></param>
    public async Task DoChangeStatusWooAsync(DataSource dataSource, List<Product> products)
    {
        // Update the products are not found in the latest crawl
        if (products.IsNotNullOrEmpty())
        {
            var wc = await InitWCObject(dataSource);

            foreach (var product in products)
            {
                using var auditingScope = _auditingManager.BeginScope();
                try
                {
                    var checkProduct = (await wc.Product.GetAll(new Dictionary<string, string>()
                    {
                        { "sku", product.Code }
                    })).FirstOrDefault();
                    if (checkProduct is not null && checkProduct.status == "publish")
                    {
                        checkProduct.status = "pending";
                        await wc.Product.Update(checkProduct.id.To<int>(), checkProduct);

                        Console.WriteLine($"Update product status: {checkProduct.name} - {checkProduct.sku} - {checkProduct.status}");
                    }
                }
                catch (Exception ex)
                {
                    LogException(_auditingManager.Current.Log, ex, product, dataSource.Url, "DoChangeStatusWooAsync");
                }
                finally
                {
                    //Always save the log
                    await auditingScope.SaveAsync();
                }
            }
        }
        else
        {
            Console.WriteLine($"Not found product to update status to pending");
        }
    }

    /// <summary>
    /// Re sync product to woo to update name, price, content
    /// </summary>
    /// <param name="checkProduct"></param>
    /// <param name="productNav"></param>
    /// <param name="wcObject"></param>
    public async Task DoReSyncProductToWooAsync(DataSource dataSource,
        WooCommerceNET.WooCommerce.v3.Product checkProduct,
        ProductWithNavigationProperties productNav,
        WCObject wcObject)
    {
        var product = productNav.Product;
        // update name
        checkProduct.name = product.Name;

        // update variants, price
        var variants = productNav.Variants;
        if (variants != null)
        {
            decimal? productPrice = variants.Count > 1 ? null : variants.FirstOrDefault()?.RetailPrice;
            decimal? discountedPrice = variants.Count > 1 ? null : variants.FirstOrDefault()?.DiscountedPrice;

            if (productPrice.HasValue && productPrice > 0)
            {
                checkProduct.price = productPrice;
                checkProduct.regular_price = productPrice;
            }

            if (discountedPrice.HasValue && discountedPrice > 0)
            {
                checkProduct.sale_price = discountedPrice;
            }
        }

        if (variants is { Count: > 1 })
        {
            var wooProductVariants = await wcObject.Product.Variations.GetAll(checkProduct.id);
            foreach (var variant in variants)
            {
                var checkVariant = wooProductVariants.FirstOrDefault(x => x.sku == variant.SKU);
                if (checkVariant != null)
                {
                    checkVariant.price = variant.RetailPrice;
                    checkVariant.regular_price = variant.RetailPrice;
                    checkVariant.sale_price = variant.DiscountedPrice;
                    await wcObject.Product.Variations.Update(checkVariant.id.To<int>(), checkVariant, checkProduct.id.To<int>());
                }
                else
                {
                    var wooVariantResult = await wcObject.Product.Variations.Add(new Variation()
                        {
                            sku = variant.SKU,
                            price = variant.RetailPrice,
                            regular_price = variant.RetailPrice,
                            sale_price = variant.DiscountedPrice
                        },
                        0);
                    if (wooVariantResult.id is > 0)
                    {
                        checkProduct.variations.Add(wooVariantResult.id.To<int>());
                    }
                }
            }
        }

        // update media + content
        await AddProductMedias(dataSource, productNav, checkProduct);

        // save product
        await wcObject.Product.Update(checkProduct.id.To<int>(), checkProduct);

        Console.WriteLine($"Update product: {checkProduct.name}");
    }

    public async Task<List<WooCommerceNET.WooCommerce.v3.Product>> GetAllProducts(WCObject wcObject, string search = "")
    {
        var checkProducts = new List<WooCommerceNET.WooCommerce.v3.Product>();
        var pageIndex = 1;

        while (true)
        {
            var checkProduct = await wcObject.Product.GetAll(new Dictionary<string, string>() { { "page", pageIndex.ToString() }, { "per_page", "100" }, { "search", search }, });
            if (checkProduct.IsNullOrEmpty()) break;

            checkProducts.AddRange(checkProduct);
            Console.WriteLine($"Fetching Product: page {pageIndex}");
            pageIndex++;
        }

        Console.WriteLine($"Fetch Product Done: {checkProducts.Count}");

        return checkProducts;
    }

    public async Task<WCObject> InitWCObject(DataSource dataSource)
    {
        var rest = new RestAPI($"{dataSource.PostToSite}/wp-json/wc/v3/", dataSource.Configuration.ApiKey, dataSource.Configuration.ApiSecret);
        var wcObject = new WCObject(rest);

        return wcObject;
    }
}