using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using LC.Crawler.BackOffice.Categories;
using LC.Crawler.BackOffice.DataSources;
using LC.Crawler.BackOffice.Extensions;
using LC.Crawler.BackOffice.Medias;
using LC.Crawler.BackOffice.ProductComments;
using LC.Crawler.BackOffice.ProductReviews;
using LC.Crawler.BackOffice.Products;
using Microsoft.Extensions.Logging;
using Volo.Abp.Auditing;
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
        IProductCommentSieuThiSongKhoeRepository productCommentSieuThiSongKhoeRepository)
    {
        _productRepository = productRepository;
        _dataSourceRepository = dataSourceRepository;
        _mediaSieuThiSongKhoeRepository = mediaSieuThiSongKhoeRepository;
        _categorySieuThiSongKhoeRepository = categorySieuThiSongKhoeRepository;
        _wooManangerBase = wooManangerBase;
        _auditingManager = auditingManager;
        _productReviewSieuThiSongKhoeRepository = productReviewSieuThiSongKhoeRepository;
        _productCommentSieuThiSongKhoeRepository = productCommentSieuThiSongKhoeRepository;
    }

    public async Task DoSyncUpdateProduct()
    {
        _dataSource = await _dataSourceRepository.GetAsync(x => x.Url.Contains(PageDataSourceConsts.SieuThiSongKhoeUrl));
        if (_dataSource == null)
        {
            return;
        }

        var rest = new RestAPI($"{_dataSource.PostToSite}/wp-json/wc/v3/", _dataSource.Configuration.ApiKey, _dataSource.Configuration.ApiSecret);
        var wc = new WCObject(rest);

        var categories = (await _categorySieuThiSongKhoeRepository.GetListAsync()).ToList();
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

    public async Task DoSyncReviews()
    {
        try
        {
            _dataSource = await _dataSourceRepository.GetAsync(x => x.Url.Contains(PageDataSourceConsts.SieuThiSongKhoeUrl));
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
        
            foreach (var product in products)
            {
                var productReviews = await _productReviewSieuThiSongKhoeRepository.GetListAsync(x => x.IsSynced == false);
                var productComments = await _productCommentSieuThiSongKhoeRepository.GetListAsync(x => x.IsSynced == false);
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

        var categories = await _categorySieuThiSongKhoeRepository.GetListAsync();
        await _wooManangerBase.SyncCategoriesAsync(_dataSource, categories);
    }

    public async Task DoSyncProductToWooAsync()
    {
        _dataSource = await _dataSourceRepository.GetAsync(x => x.Url.Contains(PageDataSourceConsts.SieuThiSongKhoeUrl));
        if (_dataSource == null)
        {
            return;
        }

        var rest = new RestAPI($"{_dataSource.PostToSite}/wp-json/wc/v3/",
            _dataSource.Configuration.ApiKey,
            _dataSource.Configuration.ApiSecret);
        var wc = new WCObject(rest);

        var wooCategories = await _wooManangerBase.GetWooCategories(_dataSource);
        var productTags = await _wooManangerBase.GetWooProductTagsAsync(_dataSource);
        var productIds = (await _productRepository.GetQueryableAsync())
            .Where(x => x.DataSourceId == _dataSource.Id 
                        //&& x.ExternalId == null
                        ).Select(x => x.Id).ToList();

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
                    await _mediaSieuThiSongKhoeRepository.UpdateManyAsync(productNav.Medias);

                    Debug.WriteLine($"Product -> {number}");
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
    }
}