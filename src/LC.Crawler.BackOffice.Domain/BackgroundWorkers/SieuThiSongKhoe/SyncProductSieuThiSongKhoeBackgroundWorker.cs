﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using LC.Crawler.BackOffice.Categories;
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
    private readonly WooManagerAladin _wooManagerAladin;

    private ILogger<SyncProductSieuThiSongKhoeBackgroundWorker> _logger;

    public SyncProductSieuThiSongKhoeBackgroundWorker(WooManagerAladin wooManagerAladin, ILogger<SyncProductSieuThiSongKhoeBackgroundWorker> logger)
    {
        _wooManagerAladin = wooManagerAladin;
        _logger = logger;
        RecurringJobId            = nameof(SyncProductSieuThiSongKhoeBackgroundWorker);
        CronExpression            = Cron.Daily(0,0);
    }
    
    public override async Task DoWorkAsync()
    {
        await _wooManagerAladin.DoSyncProductToWooAsync();
    }
}