using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;
using LC.Crawler.BackOffice.Categories;
using LC.Crawler.BackOffice.DataSources;
using LC.Crawler.BackOffice.Enums;
using LC.Crawler.BackOffice.Medias;
using LC.Crawler.BackOffice.ProductComments;
using LC.Crawler.BackOffice.ProductReviews;
using LC.Crawler.BackOffice.Products;
using Microsoft.Extensions.Logging;
using Volo.Abp.Auditing;
using WooCommerceNET;
using WooCommerceNET.WooCommerce.v3;
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
    
    private readonly IProductReviewLongChauRepository _productReviewLongChauRepository;
    private readonly IProductCommentLongChauRepository _productCommentLongChauRepository;

    private DataSource _dataSource;

    public WooManagerLongChau(IProductLongChauRepository productRepository,
        IDataSourceRepository                            dataSourceRepository,
        IMediaLongChauRepository                         mediaLongChauRepository,
        ICategoryLongChauRepository                      categoryLongChauRepository,
        WooManangerBase                                  wooManangerBase,
        IAuditingManager                                 auditingManager,
        IProductReviewLongChauRepository productReviewLongChauRepository,
        IProductCommentLongChauRepository productCommentLongChauRepository)
    {
        _productRepository                  = productRepository;
        _dataSourceRepository               = dataSourceRepository;
        _mediaLongChauRepository            = mediaLongChauRepository;
        _categoryLongChauRepository         = categoryLongChauRepository;
        _wooManangerBase                    = wooManangerBase;
        _auditingManager                    = auditingManager;
        _productReviewLongChauRepository = productReviewLongChauRepository;
        _productCommentLongChauRepository = productCommentLongChauRepository;
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

        var rest = new RestAPI($"{_dataSource.PostToSite}/wp-json/wc/v3/", _dataSource.Configuration.ApiKey, _dataSource.Configuration.ApiSecret);
        var wc = new WCObject(rest);

        var categories = ( await _categoryLongChauRepository.GetListAsync(x=>x.Name.Contains("&") && x.CategoryType == CategoryType.Ecom)).ToList();
        var wooCategories = await _wooManangerBase.GetWooCategories(_dataSource);
        //var productTags = await _wooManangerBase.GetWooProductTagsAsync(_dataSource);
        foreach (var categoryItem in categories)
        {
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
                {
                    var category = productNav.Categories.FirstOrDefault();
                    if (category != null)
                    {
                        
                        var cateTerms = category.Name.Split("->").LastOrDefault();
                        //Thuốc -> Vitamin &amp; khoáng chất
                        //Thực phẩm chức năng -> Vitamin &amp; khoáng chất
                        var encodeName = cateTerms?.Replace("&", "&amp;").Trim();
                
                        var wooCategory = wooCategories.FirstOrDefault(x =>
                            encodeName != null && x.name.Contains(encodeName, StringComparison.InvariantCultureIgnoreCase));
                        if (encodeName != null )
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
                                    if ( rootParent != null && category.Name.Contains(rootParent.name))
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
                }
            }
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

            var rest = new RestAPI($"{_dataSource.PostToSite}/wp-json/wc/v3/", _dataSource.Configuration.ApiKey,
                _dataSource.Configuration.ApiSecret);
            var wc = new WCObject(rest);
            var products = (await _productRepository.GetQueryableAsync())
                .Where(x => x.DataSourceId == _dataSource.Id
                    && x.ExternalId != null
                )
                .ToList().ToList();

            var count = 1;
            foreach (var product in products)
            {
                var productReviews = await _productReviewLongChauRepository.GetListAsync(x => x.IsSynced == false && x.ProductId == product.Id);
                var productComments = await _productCommentLongChauRepository.GetListAsync(x => x.IsSynced == false && x.ProductId == product.Id);
                
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
                Console.WriteLine($"Product Count: {count}/{products.Count}");
                count++;
            }
        }
        catch (Exception e)
        {
            Logger.LogException(e);
        }
    }
    
    public async Task DoSyncProductToWooAsync()
    {
        _dataSource = await _dataSourceRepository.GetAsync(x => x.Url.Contains(PageDataSourceConsts.LongChauUrl));
        if (_dataSource == null)
        {
            return;
        }

        var rest = new RestAPI($"{_dataSource.PostToSite}/wp-json/wc/v3/", _dataSource.Configuration.ApiKey, _dataSource.Configuration.ApiSecret);
        var wc = new WCObject(rest);

        var wooCategories = await _wooManangerBase.GetWooCategories(_dataSource);
        var productTags = await _wooManangerBase.GetWooProductTagsAsync(_dataSource);
        var productIds = (await _productRepository.GetQueryableAsync()).Where(x => x.DataSourceId == _dataSource.Id 
                                                                                   && x.ExternalId == null
                                                                                   ).Select(x=>x.Id).ToList();

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
    }
}