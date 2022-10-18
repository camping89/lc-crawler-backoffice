using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using HtmlAgilityPack;
using LC.Crawler.BackOffice.Articles;
using LC.Crawler.BackOffice.Categories;
using LC.Crawler.BackOffice.DataSources;
using LC.Crawler.BackOffice.Medias;
using LC.Crawler.BackOffice.Wordpress;
using Volo.Abp.BackgroundWorkers.Hangfire;
using WordPressPCL;
using WordPressPCL.Models;
using WooCategory = WordPressPCL.Models.Category;
using Guid = System.Guid;

namespace LC.Crawler.BackOffice.BackgroundWorkers.AloBacSi;

public class SyncArticleAloBacSiBackgroundWorker : HangfireBackgroundWorkerBase
{
    private readonly WordpressManagerAloBacSi _wordpressManagerAloBacSi;
    
    public SyncArticleAloBacSiBackgroundWorker(WordpressManagerAloBacSi wordpressManagerAloBacSi)
    {
        _wordpressManagerAloBacSi = wordpressManagerAloBacSi;

        RecurringJobId            = nameof(SyncArticleAloBacSiBackgroundWorker);
        CronExpression            = Cron.Daily(0,0);
    }

    public override async Task DoWorkAsync()
    {
        await _wordpressManagerAloBacSi.DoSyncToWordpress();
    }
}