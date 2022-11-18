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

namespace LC.Crawler.BackOffice.BackgroundWorkers.SieuThiSongKhoe;

public class SyncProductSieuThiSongKhoeBackgroundWorker : HangfireBackgroundWorkerBase
{
    private readonly WooManagerSieuThiSongKhoe _wooManagerSieuThiSongKhoe;

    private ILogger<SyncProductSieuThiSongKhoeBackgroundWorker> _logger;

    public SyncProductSieuThiSongKhoeBackgroundWorker(WooManagerSieuThiSongKhoe wooManagerSieuThiSongKhoe, ILogger<SyncProductSieuThiSongKhoeBackgroundWorker> logger)
    {
        _wooManagerSieuThiSongKhoe = wooManagerSieuThiSongKhoe;
        _logger = logger;
        RecurringJobId            = "Sync_Product_SieuThiSongKhoe_BackgroundWorker";
        CronExpression            = Cron.Daily(GlobalConfig.Crawler.SyncTimeHours,0);
    }
    
    public override async Task DoWorkAsync()
    {
        await _wooManagerSieuThiSongKhoe.DoSyncCategoriesAsync();
        await _wooManagerSieuThiSongKhoe.DoSyncProductToWooAsync();
        await _wooManagerSieuThiSongKhoe.DoSyncReviews();
    }
}

public class ReSyncProductSieuThiSongKhoeBackgroundWorker : HangfireBackgroundWorkerBase
{
    private readonly WooManagerSieuThiSongKhoe _wooManagerSieuThiSongKhoe;

    public ReSyncProductSieuThiSongKhoeBackgroundWorker(WooManagerSieuThiSongKhoe wooManagerSieuThiSongKhoe)
    {
        _wooManagerSieuThiSongKhoe = wooManagerSieuThiSongKhoe;
        RecurringJobId             = "ReSync_Product_SieuThiSongKhoe_BackgroundWorker";
        CronExpression             = Cron.Daily(GlobalConfig.Crawler.ReSyncTimeHours, 0);
    }
    
    public override async Task DoWorkAsync()
    {
        await _wooManagerSieuThiSongKhoe.DoReSyncProductToWooAsync();
    }
}