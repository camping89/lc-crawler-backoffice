﻿using System.Threading.Tasks;
using Hangfire;
using LC.Crawler.BackOffice.Wordpress;
using Volo.Abp.BackgroundWorkers.Hangfire;

namespace LC.Crawler.BackOffice.BackgroundWorkers.Aladin;

public class SyncArticleAladinBackgroundWorker : HangfireBackgroundWorkerBase
{
    private readonly WordpressManagerAladin _wordpressManagerAladin;
    public SyncArticleAladinBackgroundWorker(WordpressManagerAladin wordpressManagerAladin)
    {
        _wordpressManagerAladin = wordpressManagerAladin;
        RecurringJobId            = "Sync_Article_Aladin_BackgroundWorker";
        CronExpression            = Cron.Daily(0,0);
    }

    public override async Task DoWorkAsync()
    {
        await _wordpressManagerAladin.DoSyncCategoriesAsync();
        await _wordpressManagerAladin.DoSyncPostAsync();
    }
}