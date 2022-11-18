using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using LC.Crawler.BackOffice.Articles;
using LC.Crawler.BackOffice.Configs;
using LC.Crawler.BackOffice.DataSources;
using LC.Crawler.BackOffice.Medias;
using LC.Crawler.BackOffice.Products;
using LC.Crawler.BackOffice.WooCommerces;
using Volo.Abp.BackgroundWorkers.Hangfire;
using WooCommerceNET;
using WooCommerceNET.WooCommerce.v3;
using WooCommerceNET.WooCommerce.v3.Extension;
using WordPressPCL;
using WordPressPCL.Models;
using WooProduct = WooCommerceNET.WooCommerce.v3.Product;

namespace LC.Crawler.BackOffice.BackgroundWorkers.LongChau;

public class SyncProductLongChauBackgroundWorker : HangfireBackgroundWorkerBase
{
    private readonly WooManagerLongChau _wooManagerLongChau;
    
    public SyncProductLongChauBackgroundWorker(WooManagerLongChau wooManagerLongChau)
    {
        _wooManagerLongChau = wooManagerLongChau;
        RecurringJobId = "Sync_Product_LongChau_BackgroundWorker";
        CronExpression            = Cron.Daily(GlobalConfig.Crawler.SyncTimeHours,0);
    }

    public override async Task DoWorkAsync()
    {
        await _wooManagerLongChau.DoSyncCategoriesAsync();
        await _wooManagerLongChau.DoSyncProductToWooAsync();
        await _wooManagerLongChau.DoSyncReviews();
    }
}

public class ReSyncProductLongChauBackgroundWorker : HangfireBackgroundWorkerBase
{
    private readonly WooManagerLongChau _wooManagerLongChau;
    
    public ReSyncProductLongChauBackgroundWorker(WooManagerLongChau wooManagerLongChau)
    {
        _wooManagerLongChau = wooManagerLongChau;
        RecurringJobId      = "ReSync_Product_LongChau_BackgroundWorker";
        CronExpression      = Cron.Daily(GlobalConfig.Crawler.ReSyncTimeHours, 0);
    }

    public override async Task DoWorkAsync()
    {
        await _wooManagerLongChau.DoReSyncProductToWooAsync();
    }
}