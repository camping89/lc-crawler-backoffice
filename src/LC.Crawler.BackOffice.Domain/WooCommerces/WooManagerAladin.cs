using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using LC.Crawler.BackOffice.Categories;
using LC.Crawler.BackOffice.DataSources;
using LC.Crawler.BackOffice.Enums;
using LC.Crawler.BackOffice.Medias;
using LC.Crawler.BackOffice.ProductComments;
using LC.Crawler.BackOffice.ProductReviews;
using LC.Crawler.BackOffice.Products;
using Microsoft.Extensions.Logging;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Services;
using WooCommerceNET;
using WooCommerceNET.WooCommerce.v3;

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

    private string BASEURL = string.Empty;

    private DataSource _dataSource;

    public WooManagerAladin(IProductAladinRepository productRepository,
        IDataSourceRepository dataSourceRepository,
        WooManangerBase wooManangerBase,
        IMediaAladinRepository mediaAladinRepository,
        ICategoryAladinRepository categoryAladinRepository, IAuditingManager auditingManager,
        IProductReviewAladinRepository productReviewAladinRepository,
        IProductCommentAladinRepository productCommentAladinRepository)
    {
        _productRepository = productRepository;
        _dataSourceRepository = dataSourceRepository;
        _wooManangerBase = wooManangerBase;
        _mediaAladinRepository = mediaAladinRepository;
        _categoryAladinRepository = categoryAladinRepository;
        _auditingManager = auditingManager;
        _productReviewAladinRepository = productReviewAladinRepository;
        _productCommentAladinRepository = productCommentAladinRepository;
    }
    
    public async Task DoSyncUpdateProduct()
    {
        _dataSource = await _dataSourceRepository.GetAsync(x => x.Url.Contains(PageDataSourceConsts.AladinUrl));
        if (_dataSource == null)
        {
            return;
        }

        var rest = new RestAPI($"{_dataSource.PostToSite}/wp-json/wc/v3/", _dataSource.Configuration.ApiKey, _dataSource.Configuration.ApiSecret);
        var wc = new WCObject(rest);

        var categories = ( await _categoryAladinRepository.GetListAsync(x => x.CategoryType == CategoryType.Ecom)).ToList();
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
                var productReviews = await _productReviewAladinRepository.GetListAsync(x => x.IsSynced == false);
                var productComments = await _productCommentAladinRepository.GetListAsync(x => x.IsSynced == false);
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
        _dataSource = await _dataSourceRepository.GetAsync(x => x.Url.Contains(PageDataSourceConsts.AladinUrl));
        if (_dataSource == null)
        {
            return;
        }

        var rest = new RestAPI($"{_dataSource.PostToSite}/wp-json/wc/v3/", _dataSource.Configuration.ApiKey,
            _dataSource.Configuration.ApiSecret);
        var wc = new WCObject(rest);

        var wooCategories = await _wooManangerBase.GetWooCategories(_dataSource);
        var productTags = await _wooManangerBase.GetWooProductTagsAsync(_dataSource);
        var productIds = (await _productRepository.GetQueryableAsync())
            .Where(x => x.DataSourceId == _dataSource.Id
                        && x.ExternalId == null
                        )
            .ToList().Select(x => x.Id).ToList();
        
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
    }
}