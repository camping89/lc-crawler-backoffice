﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;
using LC.Crawler.BackOffice.Categories;
using LC.Crawler.BackOffice.DataSources;
using LC.Crawler.BackOffice.Medias;
using LC.Crawler.BackOffice.Products;
using Volo.Abp.Auditing;
using WooCommerceNET;
using WooCommerceNET.WooCommerce.v3;

namespace LC.Crawler.BackOffice.WooCommerces;

public class WooManagerLongChau : DomainService
{
    private readonly ICategoryLongChauRepository         _categoryLongChauRepository;
    private readonly IProductLongChauRepository          _productRepository;
    private readonly IDataSourceRepository               _dataSourceRepository;
    private readonly IMediaLongChauRepository            _mediaLongChauRepository;
    private readonly WooManangerBase                     _wooManangerBase;
    private readonly IAuditingManager                    _auditingManager;

    private DataSource _dataSource;

    public WooManagerLongChau(IProductLongChauRepository productRepository,
        IDataSourceRepository                            dataSourceRepository,
        IMediaLongChauRepository                         mediaLongChauRepository,
        ICategoryLongChauRepository                      categoryLongChauRepository,
        WooManangerBase                                  wooManangerBase,
        IAuditingManager                                 auditingManager)
    {
        _productRepository                  = productRepository;
        _dataSourceRepository               = dataSourceRepository;
        _mediaLongChauRepository            = mediaLongChauRepository;
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
        var productTags = await _wooManangerBase.GetWooProductTagsAsync(_dataSource);
        var productIds = (await _productRepository.GetQueryableAsync()).Where(x => x.DataSourceId == _dataSource.Id && x.ExternalId == null).Select(x=>x.Id).ToList();

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