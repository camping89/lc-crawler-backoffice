using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using HtmlAgilityPack;
using Volo.Abp.Domain.Services;
using LC.Crawler.BackOffice.Categories;
using LC.Crawler.BackOffice.Core;
using LC.Crawler.BackOffice.DataSources;
using LC.Crawler.BackOffice.Helpers;
using LC.Crawler.BackOffice.Medias;
using LC.Crawler.BackOffice.ProductAttributes;
using LC.Crawler.BackOffice.Products;
using LC.Crawler.BackOffice.ProductVariants;
using Microsoft.Extensions.Logging;
using Volo.Abp.Auditing;
using Volo.Abp.BackgroundWorkers.Hangfire;
using Volo.Abp.Domain.Repositories;
using WooCommerceNET;
using WooCommerceNET.WooCommerce.v3;
using WordPressPCL;
using WordPressPCL.Models;
using WordPressPCL.Models.Exceptions;
using Category = WordPressPCL.Models.Category;
using Product = LC.Crawler.BackOffice.Products.Product;
using WooProductCategory = WooCommerceNET.WooCommerce.v3.ProductCategory;
using WooProductAttribute = WooCommerceNET.WooCommerce.v3.ProductAttribute;
using WooProduct = WooCommerceNET.WooCommerce.v3.Product;

namespace LC.Crawler.BackOffice.WooCommerces;

public class WooManagerLongChau : DomainService
{
    private readonly ICategoryLongChauRepository         _categoryLongChauRepository;
    private readonly IProductLongChauRepository          _productRepository;
    private readonly IDataSourceRepository               _dataSourceRepository;
    private readonly IMediaLongChauRepository            _mediaLongChauRepository;
    private readonly IProductVariantLongChauRepository   _productVariantLongChauRepository;
    private readonly IProductAttributeLongChauRepository _productAttributeLongChauRepository;
    private readonly WooManangerBase                     _wooManangerBase;
    private readonly IAuditingManager                    _auditingManager;

    private DataSource _dataSource;

    public WooManagerLongChau(IProductLongChauRepository productRepository,
        IDataSourceRepository                            dataSourceRepository,
        IMediaLongChauRepository                         mediaLongChauRepository,
        IProductVariantLongChauRepository                productVariantLongChauRepository,
        IProductAttributeLongChauRepository              productAttributeLongChauRepository,
        ICategoryLongChauRepository                      categoryLongChauRepository,
        WooManangerBase                                  wooManangerBase,
        IAuditingManager                                 auditingManager)
    {
        _productRepository                  = productRepository;
        _dataSourceRepository               = dataSourceRepository;
        _mediaLongChauRepository            = mediaLongChauRepository;
        _productVariantLongChauRepository   = productVariantLongChauRepository;
        _productAttributeLongChauRepository = productAttributeLongChauRepository;
        _categoryLongChauRepository         = categoryLongChauRepository;
        _wooManangerBase                    = wooManangerBase;
        _auditingManager                    = auditingManager;
    }

    public async Task DoSyncCategoriesAsync()
    {
        _dataSource = await _dataSourceRepository.GetAsync(x => x.Url.Contains(PageDataSourceConsts.LongChauUrl));
        if (_dataSource == null)
        {
            return;
        }

        var categories = await _categoryLongChauRepository.GetListAsync();
        await _wooManangerBase.SyncCategoriesAsync(_dataSource,categories);
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
        var productIds = (await _productRepository.GetQueryableAsync()).Where(x => x.DataSourceId == _dataSource.Id && x.ExternalId == null).Select(x=>x.Id);

        var number = 1;
        foreach (var productId in productIds)
        {
            using var auditingScope = _auditingManager.BeginScope();
            var       productNav    = await _productRepository.GetWithNavigationPropertiesAsync(productId);
            
            try
            {
                var wooProduct = await  _wooManangerBase.PostToWooProduct(_dataSource, wc, productNav, wooCategories);
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
                _wooManangerBase.LogException(_auditingManager.Current.Log, ex, productNav, PageDataSourceConsts.LongChauUrl);
            }
            finally
            {
                //Always save the log
                await auditingScope.SaveAsync();
            }
        }
    }

}