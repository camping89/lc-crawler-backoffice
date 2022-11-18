using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using LC.Crawler.BackOffice.Categories;
using LC.Crawler.BackOffice.Configs;
using LC.Crawler.BackOffice.DataSources;
using LC.Crawler.BackOffice.Medias;
using LC.Crawler.BackOffice.ProductAttributes;
using LC.Crawler.BackOffice.Products;
using LC.Crawler.BackOffice.ProductVariants;
using LC.Crawler.BackOffice.WooCommerces;
using Microsoft.Extensions.Logging;
using Volo.Abp.BackgroundWorkers.Hangfire;
using Volo.Abp.Domain.Repositories;
using WooCommerceNET;
using WooCommerceNET.WooCommerce.v3;
using WordPressPCL;
using WordPressPCL.Models;
using WordPressPCL.Models.Exceptions;
using Product = LC.Crawler.BackOffice.Products.Product;
using WooProductCategory = WooCommerceNET.WooCommerce.v3.ProductCategory;
using WooProductAttribute = WooCommerceNET.WooCommerce.v3.ProductAttribute;
using WooProduct = WooCommerceNET.WooCommerce.v3.Product;

namespace LC.Crawler.BackOffice.BackgroundWorkers.Aladin;

public class SyncProductAladinBackgroundWorker : HangfireBackgroundWorkerBase
{
    private readonly WooManagerAladin _wooManagerAladin;

    private ILogger<SyncProductAladinBackgroundWorker> _logger;

    public SyncProductAladinBackgroundWorker(ILogger<SyncProductAladinBackgroundWorker> logger, WooManagerAladin wooManagerAladin)
    {
        _logger = logger;
        _wooManagerAladin = wooManagerAladin;
        RecurringJobId            = "Sync_Product_Aladin_BackgroundWorker";
        CronExpression            = Cron.Daily(GlobalConfig.Crawler.SyncTimeHours,0);
    }

    public override async Task DoWorkAsync()
    {
        await _wooManagerAladin.DoSyncCategoriesAsync();
        await _wooManagerAladin.DoSyncProductToWooAsync();

        await _wooManagerAladin.DoSyncReviews();
    }

}


public class ReSyncProductAladinBackgroundWorker : HangfireBackgroundWorkerBase
{
    private readonly WooManagerAladin _wooManagerAladin;

    public ReSyncProductAladinBackgroundWorker(WooManagerAladin wooManagerAladin)
    {
        _wooManagerAladin = wooManagerAladin;
        RecurringJobId    = "ReSync_Product_Aladin_BackgroundWorker";
        CronExpression    = Cron.Daily(GlobalConfig.Crawler.ReSyncTimeHours, 0);
    }

    public override async Task DoWorkAsync()
    {
        await _wooManagerAladin.DoReSyncProductToWooAsync();
    }

}