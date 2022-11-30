using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;
using LC.Crawler.BackOffice.Categories;
using LC.Crawler.BackOffice.Core;
using LC.Crawler.BackOffice.DataSources;
using LC.Crawler.BackOffice.Enums;
using LC.Crawler.BackOffice.Extensions;
using LC.Crawler.BackOffice.Helpers;
using LC.Crawler.BackOffice.Medias;
using LC.Crawler.BackOffice.Payloads;
using LC.Crawler.BackOffice.ProductComments;
using LC.Crawler.BackOffice.ProductReviews;
using LC.Crawler.BackOffice.Products;
using LC.Crawler.BackOffice.ProductVariants;
using Microsoft.Extensions.Logging;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Repositories;
using WooCommerceNET;
using WooCommerceNET.WooCommerce.v3;
using WordPressPCL.Models;
using Product = LC.Crawler.BackOffice.Products.Product;
using ProductReview = LC.Crawler.BackOffice.ProductReviews.ProductReview;

namespace LC.Crawler.BackOffice.WooCommerces;

public class WooManagerLongChau : DomainService
{
    private readonly ICategoryLongChauRepository         _categoryLongChauRepository;
    private readonly IProductLongChauRepository          _productRepository;
    private readonly IDataSourceRepository               _dataSourceRepository;
    private readonly IMediaLongChauRepository            _mediaLongChauRepository;
    private readonly WooManangerBase                     _wooManangerBase;
    private readonly IAuditingManager                    _auditingManager;
    
    private readonly IProductReviewLongChauRepository  _productReviewLongChauRepository;
    private readonly IProductCommentLongChauRepository _productCommentLongChauRepository;
    private readonly IProductVariantLongChauRepository _productVariantLongChauRepository;
    
    private readonly DataSourceManager _dataSourceManager;

    private DataSource _dataSource;

    public WooManagerLongChau(IProductLongChauRepository productRepository,
        IDataSourceRepository                            dataSourceRepository,
        IMediaLongChauRepository                         mediaLongChauRepository,
        ICategoryLongChauRepository                      categoryLongChauRepository,
        WooManangerBase                                  wooManangerBase,
        IAuditingManager                                 auditingManager,
        IProductReviewLongChauRepository                 productReviewLongChauRepository,
        IProductCommentLongChauRepository                productCommentLongChauRepository,
        IProductVariantLongChauRepository                productVariantLongChauRepository,
        DataSourceManager dataSourceManager)
    {
        _productRepository                     = productRepository;
        _dataSourceRepository                  = dataSourceRepository;
        _mediaLongChauRepository               = mediaLongChauRepository;
        _categoryLongChauRepository            = categoryLongChauRepository;
        _wooManangerBase                       = wooManangerBase;
        _auditingManager                       = auditingManager;
        _productReviewLongChauRepository       = productReviewLongChauRepository;
        _productCommentLongChauRepository      = productCommentLongChauRepository;
        _productVariantLongChauRepository = productVariantLongChauRepository;
        _dataSourceManager = dataSourceManager;
    }

    public async Task DoSyncCategoriesAsync()
    {
        _dataSource = await _dataSourceRepository.GetAsync(x => x.Url.Contains(PageDataSourceConsts.LongChauUrl));
        if (_dataSource == null)
        {
            return;
        }

        var categories = await _categoryLongChauRepository.GetListAsync(_ => _.CategoryType == CategoryType.Ecom);
        await _wooManangerBase.SyncCategoriesAsync(_dataSource,categories);
    }

    public async Task DoSyncUpdateProduct()
    {
        _dataSource = await _dataSourceRepository.GetAsync(x => x.Url.Contains(PageDataSourceConsts.LongChauUrl));
        if (_dataSource == null)
        {
            return;
        }

        // get rest api, wc object
        var wc = await _wooManangerBase.InitWCObject(_dataSource);

        var categories = ( await _categoryLongChauRepository.GetListAsync(x=>x.CategoryType == CategoryType.Ecom)).ToList();
        var wooCategories = await _wooManangerBase.GetWooCategories(_dataSource);
        //var productTags = await _wooManangerBase.GetWooProductTagsAsync(_dataSource);
        var categoryCount = 1;
        foreach (var categoryItem in categories)
        {
            //TODO remove condition ExternalId for updating product
            var productIds = (await _productRepository.GetQueryableAsync()).Where(x => x.DataSourceId == _dataSource.Id && x.ExternalId != null
                && x.Categories.Any(_ => _.CategoryId == categoryItem.Id))
            .Select(x=>x.Id).ToList();

            var productCount = 1;
            foreach (var productId in productIds)
            {
                using var auditingScope = _auditingManager.BeginScope();
                var productNav = await _productRepository.GetWithNavigationPropertiesAsync(productId);

                var checkProduct = (await wc.Product.GetAll(new Dictionary<string, string>()
                {
                    { "sku", productNav.Product.Code }
                })).FirstOrDefault();

                if (checkProduct != null)
                    //&& checkProduct.categories.Any(x=>x.slug == "uncategorized"))
                {
                    var category = productNav.Categories.FirstOrDefault();
                    if (category != null)
                    {
                        
                        var cateTerms = category.Name.Split("->").LastOrDefault();
                        //Thuốc -> Vitamin &amp; khoáng chất
                        //Thực phẩm chức năng -> Vitamin &amp; khoáng chất
                        var encodeName = cateTerms?.Replace("&", "&amp;").Trim();
                        
                        if (category.Name.Contains("->"))
                        {
                            var wooCategory = wooCategories.FirstOrDefault(x =>
                                encodeName != null && x.name.Equals(encodeName, StringComparison.InvariantCultureIgnoreCase));
                            if (encodeName != null )
                            {
                                category.Name = category.Name.Replace("&", "&amp;").Trim();
                                var wooCategoriesFilter = wooCategories.Where(x =>
                                    x.name.Equals(encodeName, StringComparison.InvariantCultureIgnoreCase)).ToList();
                                foreach (var wooCate in wooCategoriesFilter)
                                {
                                    var parentCate = wooCategories.FirstOrDefault(x => x.id == wooCate.parent);
                                    if (parentCate != null && category.Name.Contains(parentCate.name, StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        var rootParent = wooCategories.FirstOrDefault(x => x.id == parentCate.parent);
                                        if ( (rootParent != null && category.Name.Contains(rootParent.name, StringComparison.InvariantCultureIgnoreCase)) || parentCate.parent == 0)
                                        {
                                            wooCategory = wooCate;
                                            checkProduct.categories = new List<ProductCategoryLine>()
                                            {
                                                new()
                                                {
                                                    id = wooCategory.id,
                                                    name = wooCategory.name,
                                                    slug = wooCategory.slug
                                                }
                                            };
                                            await wc.Product.Update(checkProduct.id.To<int>(), checkProduct);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            var wooCategory = wooCategories.FirstOrDefault(x =>
                                encodeName != null && x.name.Equals(encodeName, StringComparison.InvariantCultureIgnoreCase) && x.parent == 0);
                            if (wooCategory is not null)
                            {
                                checkProduct.categories = new List<ProductCategoryLine>()
                                {
                                    new()
                                    {
                                        id = wooCategory.id,
                                        name = wooCategory.name,
                                        slug = wooCategory.slug
                                    }
                                };
                                await wc.Product.Update(checkProduct.id.To<int>(), checkProduct);
                            }
                        }
                    }
                }
                
                Console.WriteLine($"Sync Product: {productCount}/{productIds.Count}");
                productCount++;
            }
            Console.WriteLine($"Sync Category: {categoryCount}/{categories.Count}");
            categoryCount++;
        }
    }

    public async Task DoSyncReviews()
    {
        try
        {
            _dataSource = await _dataSourceRepository.GetAsync(x => x.Url.Contains(PageDataSourceConsts.LongChauUrl));
            if (_dataSource == null)
            {
                return;
            }

            // get rest api, wc object
            var wc = await _wooManangerBase.InitWCObject(_dataSource);
            
            var reviews = await _productReviewLongChauRepository.GetListAsync(x => !x.IsSynced);
            var comments = await _productCommentLongChauRepository.GetListAsync(x => !x.IsSynced);
            
            if (reviews.IsNullOrEmpty() && comments.IsNullOrEmpty()) return;
            
            var products = (await _productRepository.GetQueryableAsync())
                .Where(x => x.DataSourceId == _dataSource.Id
                    && x.ExternalId != null
                )
                .ToList().ToList();
            
            foreach (var product in products)
            {
                var productReviews = reviews.Where(x => x.ProductId == product.Id).ToList();
                var productComments = comments.Where(x => x.ProductId == product.Id).ToList();
                
                if(productReviews.IsNullOrEmpty() && productComments.IsNullOrEmpty()) continue;
                
                await _wooManangerBase.PostProductReviews(wc, product.Code, productComments, productReviews);
                foreach (var productReview in productReviews)
                {
                    productReview.IsSynced = true;
                }
                foreach (var productComment in productComments)
                {
                    productComment.IsSynced = true;
                }

                if (productReviews.Any())
                {
                    await _productReviewLongChauRepository.UpdateManyAsync(productReviews);
                }
                if (productComments.Any())
                {
                    await _productCommentLongChauRepository.UpdateManyAsync(productComments);
                }
            }
        }
        catch (Exception e)
        {
            Logger.LogException(e);
        }
    }
    
    public async Task DoSyncProductToWooAsync()
    {
        // get datasource
        _dataSource = await _dataSourceRepository.GetAsync(x => x.Url.Contains(PageDataSourceConsts.LongChauUrl));
        if (_dataSource == null || !_dataSource.ShouldSyncProduct)
        {
            return;
        }
        
        // update re-sync status
        await _dataSourceManager.DoUpdateSyncStatus(_dataSource.Id, PageSyncStatusType.SyncProduct, PageSyncStatus.InProgress);

        // get rest api, wc object
        var wc = await _wooManangerBase.InitWCObject(_dataSource);

        // get woo categories, product tags, product ids
        var wooCategories = await _wooManangerBase.GetWooCategories(_dataSource);
        var productTags = await _wooManangerBase.GetWooProductTagsAsync(_dataSource);
        var productIds = (await _productRepository.GetQueryableAsync()).Where(x => x.DataSourceId == _dataSource.Id 
                                                                                   && x.Name != null
                                                                                   && x.Code != null
                                                                                   && x.ExternalId == null
                                                                                   ).Select(x=>x.Id).ToList();

        // sync product to wp
        var number = 1;
        foreach (var productId in productIds)
        {
            using var auditingScope = _auditingManager.BeginScope();
            var       productNav    = await _productRepository.GetWithNavigationPropertiesAsync(productId);
            
            try
            {
                var wooProduct = await  _wooManangerBase.PostToWooProduct(_dataSource, wc, productNav, wooCategories,productTags);
                if (wooProduct is { id: > 0 })
                {
                    productNav.Product.ExternalId = wooProduct.id.To<int>();
                    await _productRepository.UpdateAsync(productNav.Product, true);
                    await _mediaLongChauRepository.UpdateManyAsync(productNav.Medias);
                    
                    Debug.WriteLine($"Product -> {number}");
                    number++;
                }
            }
            catch (Exception ex)
            {
                //Add exceptions
                _wooManangerBase.LogException(_auditingManager.Current.Log, ex, productNav.Product, PageDataSourceConsts.LongChauUrl);
            }
            finally
            {
                //Always save the log
                await auditingScope.SaveAsync();
            }
        }
        
        // update re-sync status
        await _dataSourceManager.DoUpdateSyncStatus(_dataSource.Id, PageSyncStatusType.SyncProduct, PageSyncStatus.Completed);
    }
    
    public async Task DoReSyncProductToWooAsync()
    {
        // get data source
        _dataSource = await _dataSourceRepository.GetAsync(x => x.Url.Contains(PageDataSourceConsts.LongChauUrl));
        if (_dataSource == null || !_dataSource.ShouldReSyncProduct)
        {
            return;
        }
        
        // update re-sync status
        await _dataSourceManager.DoUpdateSyncStatus(_dataSource.Id, PageSyncStatusType.ResyncProduct, PageSyncStatus.InProgress);
        
        // get rest api, wc object
        var wcObject = await _wooManangerBase.InitWCObject(_dataSource);

        // get all products
        var checkProducts = await _wooManangerBase.GetAllProducts(wcObject);

        Console.WriteLine($"Fetch Product Done: {checkProducts.Count}");

        // Update wo products
        foreach (var checkProduct in checkProducts)
        {
            using var auditingScope = _auditingManager.BeginScope();
            
            try
            {
                var product = await _productRepository.FirstOrDefaultAsync(_ => _.ExternalId == checkProduct.id.To<int>());
                if (product is null)
                {
                    continue;
                }
                
                var productNav = await _productRepository.GetWithNavigationPropertiesAsync(product.Id);
                await _wooManangerBase.DoReSyncProductToWooAsync(_dataSource, checkProduct, productNav, wcObject);
            }
            catch (Exception ex)
            {
                //Add exceptions
                _wooManangerBase.LogException(_auditingManager.Current.Log, ex, new Product(),
                                              PageDataSourceConsts.LongChauUrl, "DoReSyncProductToWooAsync");
            }
            finally
            {
                //Always save the log
                await auditingScope.SaveAsync();
            }
        }
        
        // update re-sync status
        await _dataSourceManager.DoUpdateSyncStatus(_dataSource.Id, PageSyncStatusType.ResyncProduct, PageSyncStatus.Completed);
    }

    /// <summary>
    ///  Update the products are not found in the latest crawl
    /// </summary>
    /// <param name="products"></param>
    public async Task DoChangeStatusWooAsync(List<CrawlEcommerceProductPayload> products)
    {
        _dataSource = await _dataSourceRepository.GetAsync(x => x.Url.Contains(PageDataSourceConsts.LongChauUrl));
        if (_dataSource == null)
        {
            return;
        }
        
        // Update the products are not found in the latest crawl
        var productCodes     = products.Select(_ => _.Code).ToList();
        var notFoundProducts = await _productRepository.GetListAsync(_ => !productCodes.Contains(_.Code) && _.ExternalId != null);
        
        // Change status
        await _wooManangerBase.DoChangeStatusWooAsync(_dataSource, notFoundProducts);
    }
}