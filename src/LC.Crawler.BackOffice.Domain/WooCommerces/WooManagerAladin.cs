using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using LC.Crawler.BackOffice.Categories;
using LC.Crawler.BackOffice.DataSources;
using LC.Crawler.BackOffice.Enums;
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

public class WooManagerAladin : DomainService
{
    private readonly ICategoryAladinRepository _categoryAladinRepository;
    private readonly IProductAladinRepository _productRepository;
    private readonly IDataSourceRepository _dataSourceRepository;
    private readonly IMediaAladinRepository _mediaAladinRepository;
    private readonly WooManangerBase _wooManangerBase;
    private readonly IAuditingManager _auditingManager;
    
    private readonly IProductReviewAladinRepository _productReviewAladinRepository;
    private readonly IProductCommentAladinRepository _productCommentAladinRepository;
    
    private readonly DataSourceManager _dataSourceManager;

    private string BASEURL = string.Empty;

    private DataSource _dataSource;

    public WooManagerAladin(IProductAladinRepository productRepository,
        IDataSourceRepository dataSourceRepository,
        WooManangerBase wooManangerBase,
        IMediaAladinRepository mediaAladinRepository,
        ICategoryAladinRepository categoryAladinRepository, IAuditingManager auditingManager,
        IProductReviewAladinRepository productReviewAladinRepository,
        IProductCommentAladinRepository productCommentAladinRepository,
        DataSourceManager dataSourceManager)
    {
        _productRepository = productRepository;
        _dataSourceRepository = dataSourceRepository;
        _wooManangerBase = wooManangerBase;
        _mediaAladinRepository = mediaAladinRepository;
        _categoryAladinRepository = categoryAladinRepository;
        _auditingManager = auditingManager;
        _productReviewAladinRepository = productReviewAladinRepository;
        _productCommentAladinRepository = productCommentAladinRepository;
        _dataSourceManager = dataSourceManager;
    }
    
    public async Task DoSyncUpdateProduct()
    {
        _dataSource = await _dataSourceRepository.GetAsync(x => x.Url.Contains(PageDataSourceConsts.AladinUrl));
        if (_dataSource == null)
        {
            return;
        }

        // get rest api, wc object
        var wc      = await _wooManangerBase.InitWCObject(_dataSource);
        
        var categories    = ( await _categoryAladinRepository.GetListAsync(x => x.CategoryType == CategoryType.Ecom)).ToList();
        var wooCategories = await _wooManangerBase.GetWooCategories(_dataSource);
        //var productTags = await _wooManangerBase.GetWooProductTagsAsync(_dataSource);
        foreach (var categoryItem in categories)
        {
            //TODO remove condition ExternalId for updating product
            var productIds = (await _productRepository.GetQueryableAsync()).Where(x => x.DataSourceId == _dataSource.Id && x.ExternalId != null
                && x.Categories.Any(x=>x.CategoryId == categoryItem.Id))
            .Select(x=>x.Id).ToList();
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
                                    if (parentCate != null && category.Name.Contains(parentCate.name))
                                    {
                                        var rootParent = wooCategories.FirstOrDefault(x => x.id == parentCate.parent);
                                        if ( (rootParent != null && category.Name.Contains(rootParent.name)) || parentCate.parent == 0)
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
            }
        }
    }
    
    public async Task DoSyncTagAsync()
    {
        _dataSource = await _dataSourceRepository.GetAsync(x => x.Url.Contains(PageDataSourceConsts.AladinUrl));
        if (_dataSource == null)
        {
            return;
        }

        var tags = (await _productRepository.GetQueryableAsync())
                  .Where(x => x.Tags.Any())
                  .ToList().Select(x => x.Tags);
        
        var tagList = new List<string>();
        tagList.AddRange(tags.SelectMany(x => x));
        tagList = tagList.Distinct().ToList();
        
        await _wooManangerBase.SyncProductTagsAsync(_dataSource, tagList);
    }

    public async Task DoSyncCategoriesAsync()
    {
        _dataSource = await _dataSourceRepository.GetAsync(x => x.Url.Contains(PageDataSourceConsts.AladinUrl));
        if (_dataSource == null)
        {
            return;
        }

        var categories = await _categoryAladinRepository.GetListAsync(_ => _.CategoryType == CategoryType.Ecom);
        await _wooManangerBase.SyncCategoriesAsync(_dataSource, categories);
    }

    public async Task DoSyncReviews()
    {
        try
        {
            _dataSource = await _dataSourceRepository.GetAsync(x => x.Url.Contains(PageDataSourceConsts.AladinUrl));
            if (_dataSource == null)
            {
                return;
            }

            // get rest api, wc object
            var wc = await _wooManangerBase.InitWCObject(_dataSource);
        
            var reviews = await _productReviewAladinRepository.GetListAsync(x => !x.IsSynced);
            var comments = await _productCommentAladinRepository.GetListAsync(x => !x.IsSynced);
            
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
                    await _productReviewAladinRepository.UpdateManyAsync(productReviews);
                }
                if (productComments.Any())
                {
                    await _productCommentAladinRepository.UpdateManyAsync(productComments);
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
        // get data source
        _dataSource = await _dataSourceRepository.GetAsync(x => x.Url.Contains(PageDataSourceConsts.AladinUrl));
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
        var productIds = (await _productRepository.GetQueryableAsync())
            .Where(x => x.DataSourceId == _dataSource.Id
                        && x.Name != null
                        && x.Code != null
                        && x.ExternalId == null
                        )
            .ToList().Select(x => x.Id).ToList();
        
        // sync product to wp
        var number = 1;
        foreach (var productId in productIds)
        {
            using var auditingScope = _auditingManager.BeginScope();
            var productNav = await _productRepository.GetWithNavigationPropertiesAsync(productId);

            try
            {
                var wooProduct =
                    await _wooManangerBase.PostToWooProduct(_dataSource, wc, productNav, wooCategories, productTags);
                if (wooProduct is { id: > 0 })
                {
                    productNav.Product.ExternalId = wooProduct.id.To<int>();
                    await _productRepository.UpdateAsync(productNav.Product, true);
                    await _mediaAladinRepository.UpdateManyAsync(productNav.Medias);

                    Debug.WriteLine($"Product -> {number}");
                    number++;
                }
            }
            catch (Exception ex)
            {
                //Add exceptions
                _wooManangerBase.LogException(_auditingManager.Current.Log, ex, productNav.Product,
                    PageDataSourceConsts.AladinUrl);
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
        _dataSource = await _dataSourceRepository.GetAsync(x => x.Url.Contains(PageDataSourceConsts.AladinUrl));
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
                _wooManangerBase.LogException(_auditingManager.Current.Log,
                                              ex,
                                              new Products.Product(),
                                              PageDataSourceConsts.AladinUrl,
                                              "DoReSyncProductToWooAsync");
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
        _dataSource = await _dataSourceRepository.GetAsync(x => x.Url.Contains(PageDataSourceConsts.AladinUrl));
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

    public async Task RemoveExternalIdAsync()
    {
        var productIds = (await _productRepository.GetQueryableAsync())
                        .Where(x => x.DataSourceId == _dataSource.Id
                                 && x.Name         != null
                                 && x.Code         != null
                                 && x.ExternalId   != null
                              )
                        .ToList().Select(x => x.Id).ToList();
        // sync product to wp
        var number = 1;
        var total  = productIds.Count();
        foreach (var productId in productIds)
        {
            try
            {
                var product = await _productRepository.GetAsync(productId);
                if (product is not null)
                {
                    product.ExternalId = null;
                    await _productRepository.UpdateAsync(product, true);
                }
                
                Console.WriteLine($"Product -> {number}/{total}");
                number++;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}