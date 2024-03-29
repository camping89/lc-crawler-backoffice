﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using LC.Crawler.BackOffice.Categories;
using LC.Crawler.BackOffice.DataSources;
using LC.Crawler.BackOffice.Enums;
using LC.Crawler.BackOffice.Extensions;
using LC.Crawler.BackOffice.Helpers;
using LC.Crawler.BackOffice.Medias;
using LC.Crawler.BackOffice.Payloads;
using LC.Crawler.BackOffice.ProductComments;
using LC.Crawler.BackOffice.ProductReviews;
using LC.Crawler.BackOffice.Products;
using Microsoft.Extensions.Logging;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using WooCommerceNET;
using WooCommerceNET.WooCommerce.v3;
using Product = WooCommerceNET.WooCommerce.v3.Product;

namespace LC.Crawler.BackOffice.WooCommerces;

public class WooManagerSieuThiSongKhoe : DomainService
{
    private readonly ICategorySieuThiSongKhoeRepository _categorySieuThiSongKhoeRepository;
    private readonly IProductSieuThiSongKhoeRepository _productRepository;
    private readonly IDataSourceRepository _dataSourceRepository;
    private readonly IMediaSieuThiSongKhoeRepository _mediaSieuThiSongKhoeRepository;
    private readonly WooManangerBase _wooManangerBase;
    private readonly IAuditingManager _auditingManager;
    
    private readonly DataSourceManager _dataSourceManager;
    
    
    private readonly IProductReviewSieuThiSongKhoeRepository _productReviewSieuThiSongKhoeRepository;
    private readonly IProductCommentSieuThiSongKhoeRepository _productCommentSieuThiSongKhoeRepository;

    private string BASEURL = string.Empty;

    private DataSource _dataSource;

    public WooManagerSieuThiSongKhoe(IProductSieuThiSongKhoeRepository productRepository,
        IDataSourceRepository dataSourceRepository,
        IMediaSieuThiSongKhoeRepository mediaSieuThiSongKhoeRepository,
        ICategorySieuThiSongKhoeRepository categorySieuThiSongKhoeRepository,
        WooManangerBase wooManangerBase,
        IAuditingManager auditingManager,
        IProductReviewSieuThiSongKhoeRepository productReviewSieuThiSongKhoeRepository,
        IProductCommentSieuThiSongKhoeRepository productCommentSieuThiSongKhoeRepository,
        DataSourceManager dataSourceManager)
    {
        _productRepository = productRepository;
        _dataSourceRepository = dataSourceRepository;
        _mediaSieuThiSongKhoeRepository = mediaSieuThiSongKhoeRepository;
        _categorySieuThiSongKhoeRepository = categorySieuThiSongKhoeRepository;
        _wooManangerBase = wooManangerBase;
        _auditingManager = auditingManager;
        _productReviewSieuThiSongKhoeRepository = productReviewSieuThiSongKhoeRepository;
        _productCommentSieuThiSongKhoeRepository = productCommentSieuThiSongKhoeRepository;
        _dataSourceManager = dataSourceManager;
    }

    public async Task DoSyncUpdateProduct()
    {
        _dataSource = await _dataSourceRepository.GetAsync(x => x.Url.Contains(PageDataSourceConsts.SieuThiSongKhoeUrl));
        if (_dataSource == null)
        {
            return;
        }

        // get rest api, wc object
        var wc = await _wooManangerBase.InitWCObject(_dataSource);

        var categories = (await _categorySieuThiSongKhoeRepository.GetListAsync(_ => _.CategoryType == CategoryType.Ecom)).ToList();
        var wooCategories = await _wooManangerBase.GetWooCategories(_dataSource);
        //var productTags = await _wooManangerBase.GetWooProductTagsAsync(_dataSource);
        
        var index = 1;
        var products = new List<Product>();
        var pageIndex = 1;
        while (true)
        {
            var result = await wc.Product.GetAll(new Dictionary<string, string>()
            {
                
                //{ "category", "15" },
                { "page", pageIndex.ToString() },
                { "per_page", "100" },
            });

            if (result.IsNullOrEmpty())
            {
                break;
            }

            products.AddRange(result);
            Console.WriteLine($"Page : {pageIndex} ");
            pageIndex++;
        }

        var skus = products.Select(x => x.sku);
        var productIds = (await _productRepository.GetQueryableAsync()).Where(x=>x.ExternalId != null && skus.Contains(x.Code)).Select(x => x.Id);
        Console.WriteLine($"Total: {productIds.Count()}");
        foreach (var productId in productIds)
        {
            var productNav = await _productRepository.GetWithNavigationPropertiesAsync(productId);
            var checkProduct = products.FirstOrDefault(x=>x.sku == productNav.Product.Code);

            if (checkProduct != null)
            {
                var category = productNav.Categories.FirstOrDefault();
                if (category != null && category.Name.IsNotNullOrEmpty())
                {
                    var cateTerms = category.Name.Split("->").LastOrDefault();
                    //Thuốc -> Vitamin &amp; khoáng chất
                    //Thực phẩm chức năng -> Vitamin &amp; khoáng chất
                    var encodeName = cateTerms?.Replace("&", "&amp;").Trim();

                    var wooCategory = wooCategories.FirstOrDefault(x =>
                        encodeName != null && x.name.Contains(encodeName, StringComparison.InvariantCultureIgnoreCase));
                    if (encodeName != null)
                    {
                        category.Name = category.Name.Replace("&", "&amp;").Trim();
                        var wooCategoriesFilter = wooCategories.Where(x =>
                            x.name.Contains(encodeName, StringComparison.InvariantCultureIgnoreCase)).ToList();
                        foreach (var wooCate in wooCategoriesFilter)
                        {
                            var parentCate = wooCategories.FirstOrDefault(x => x.id == wooCate.parent);
                            if (parentCate != null && category.Name.Contains(parentCate.name))
                            {
                                var rootParent = wooCategories.FirstOrDefault(x => x.id == parentCate.parent);
                                if ( (rootParent != null && category.Name.Contains(rootParent.name)) || parentCate.parent == 0)
                                {
                                    wooCategory = wooCate;
                                    if (checkProduct.categories.All(x => x.id != wooCategory.id))
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
                                        try
                                        {
                                            await wc.Product.Update(checkProduct.id.To<int>(), checkProduct);
                                        }
                                        catch (Exception e)
                                        {
                                            Console.WriteLine(e);
                                            continue;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            Console.WriteLine($"Product: {index}");
            index++;
        }
    }
    
    public async Task DoSyncResetProduct()
    {
        _dataSource = await _dataSourceRepository.GetAsync(x => x.Url.Contains(PageDataSourceConsts.SieuThiSongKhoeUrl));
        if (_dataSource == null)
        {
            return;
        }

        // get rest api, wc object
        var wc = await _wooManangerBase.InitWCObject(_dataSource);

        var index = 1;
        var products = new List<Product>();
        var pageIndex = 1;
        while (true)
        {
            var result = await wc.Product.GetAll(new Dictionary<string, string>()
            {
                
                //{ "category", "15" },
                { "page", pageIndex.ToString() },
                { "per_page", "100" },
            });

            if (result.IsNullOrEmpty())
            {
                break;
            }

            products.AddRange(result);
            Console.WriteLine($"Page : {pageIndex} ");
            pageIndex++;
        }

        var externalIds = products.Select(x => x.id.To<int>()).ToList();
        var productIds = (await _productRepository.GetQueryableAsync()).ToList().Where(x=>x.ExternalId != null && externalIds.Contains(x.ExternalId.Value) == false).Select(x => x.Id);
        Console.WriteLine($"Total: {productIds.Count()}");
        foreach (var productId in productIds)
        {
            try
            {
                var product = await _productRepository.FirstOrDefaultAsync(x => x.Id == productId);
                if (product != null)
                {
                    product.ExternalId = null;
                    await _productRepository.UpdateAsync(product, true);
                }

                Console.WriteLine($"Product: {index}");
                index++;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }

    public async Task DoSyncReviews()
    {
        try
        {
            _dataSource = await _dataSourceRepository.GetAsync(x => x.Url.Contains(PageDataSourceConsts.SieuThiSongKhoeUrl));
            if (_dataSource == null)
            {
                return;
            }

            // get rest api, wc object
            var wc       = await _wooManangerBase.InitWCObject(_dataSource);
            
            var reviews  = await _productReviewSieuThiSongKhoeRepository.GetListAsync(x => !x.IsSynced);
            var comments = await _productCommentSieuThiSongKhoeRepository.GetListAsync(x => !x.IsSynced);
            
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
                    await _productReviewSieuThiSongKhoeRepository.UpdateManyAsync(productReviews);
                }
                if (productComments.Any())
                {
                    await _productCommentSieuThiSongKhoeRepository.UpdateManyAsync(productComments);
                }
            }
        }
        catch (Exception e)
        {
            Logger.LogException(e);
        }
    }
    
    public async Task DoSyncCategoriesAsync()
    {
        _dataSource = await _dataSourceRepository.GetAsync(x => x.Url.Contains(PageDataSourceConsts.SieuThiSongKhoeUrl));
        if (_dataSource == null)
        {
            return;
        }

        var categories = await _categorySieuThiSongKhoeRepository.GetListAsync(_ => _.CategoryType == CategoryType.Ecom);
        await _wooManangerBase.SyncCategoriesAsync(_dataSource, categories);
    }

    public async Task DoSyncProductToWooAsync()
    {
        // get data source
        _dataSource = await _dataSourceRepository.GetAsync(x => x.Url.Contains(PageDataSourceConsts.SieuThiSongKhoeUrl));
        //
        // // update re-sync status
        // await _dataSourceManager.DoUpdateSyncStatus(_dataSource.Id, PageSyncStatusType.SyncProduct, PageSyncStatus.InProgress);

        // get rest api, wc object
        var wc = await _wooManangerBase.InitWCObject(_dataSource);

        // get woo categories, product tags
        var wooCategories = await _wooManangerBase.GetWooCategories(_dataSource);
        var productTags = await _wooManangerBase.GetWooProductTagsAsync(_dataSource);
        
        //TODO remove condition ExternalId for updating product
        var productIds = (await _productRepository.GetQueryableAsync())
            .Where(x => x.DataSourceId == _dataSource.Id 
                        && x.Name != null
                        && x.Code != null
                        && x.ExternalId == null
                        ).ToList().OrderByDescending(x=>x.CreationTime).Select(x => x.Id).ToList();

        // sync product to wp
        var number = 1;
        Console.WriteLine($"Product total {productIds.Count}");
        foreach (var productId in productIds)
        {
            using var auditingScope = _auditingManager.BeginScope();
            var productNav = await _productRepository.GetWithNavigationPropertiesAsync(productId);

            try
            {
                var listContentMediaIds = StringHtmlHelper.GetContentMediaIds(productNav.Product.Description);
                var contentMedias = await _mediaSieuThiSongKhoeRepository.GetListAsync(x => listContentMediaIds.Contains(x.Id));
                var wooProduct =
                    await _wooManangerBase.PostToWooProduct(_dataSource, wc, productNav, wooCategories, productTags, contentMedias);
                if (wooProduct is { id: > 0 })
                {
                    productNav.Product.ExternalId = wooProduct.id.To<int>();
                    await _productRepository.UpdateAsync(productNav.Product, true);
                    await _mediaSieuThiSongKhoeRepository.UpdateManyAsync(productNav.Medias);

                    Console.WriteLine($"Product -> {number}");
                    number++;
                }
            }
            catch (Exception ex)
            {
                //Add exceptions
                _wooManangerBase.LogException(_auditingManager.Current.Log,
                    ex,
                    productNav.Product,
                    PageDataSourceConsts.SieuThiSongKhoeUrl);
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
        _dataSource = await _dataSourceRepository.GetAsync(x => x.Url.Contains(PageDataSourceConsts.SieuThiSongKhoeUrl));
        // if (_dataSource == null || !_dataSource.ShouldReSyncProduct)
        // {
        //     return;
        // }
        
        // update re-sync status
        //await _dataSourceManager.DoUpdateSyncStatus(_dataSource.Id, PageSyncStatusType.ResyncProduct, PageSyncStatus.InProgress);
        
        // get rest api, wc object
        var wcObject = await _wooManangerBase.InitWCObject(_dataSource);

        // get all products
        var checkProducts = await _wooManangerBase.GetAllProducts(wcObject);

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
                var listContentMediaIds = StringHtmlHelper.GetContentMediaIds(productNav.Product.Description);
                var contentMedias = await _mediaSieuThiSongKhoeRepository.GetListAsync(x => listContentMediaIds.Contains(x.Id));
                await _wooManangerBase.DoReSyncProductToWooAsync(_dataSource, checkProduct, productNav, wcObject,contentMedias);
            }
            catch (Exception ex)
            {
                //Add exceptions
                _wooManangerBase.LogException(_auditingManager.Current.Log, ex, new Products.Product(),
                                              PageDataSourceConsts.SieuThiSongKhoeUrl, "DoReSyncProductToWooAsync");
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
        _dataSource = await _dataSourceRepository.GetAsync(x => x.Url.Contains(PageDataSourceConsts.SieuThiSongKhoeUrl));
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