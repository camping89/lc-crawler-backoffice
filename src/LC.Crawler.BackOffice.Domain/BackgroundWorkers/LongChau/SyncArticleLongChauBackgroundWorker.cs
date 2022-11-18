using System.Threading.Tasks;
using Hangfire;
using LC.Crawler.BackOffice.Configs;
using LC.Crawler.BackOffice.Wordpress;
using Volo.Abp.BackgroundWorkers.Hangfire;

namespace LC.Crawler.BackOffice.BackgroundWorkers.LongChau;

public class SyncArticleLongChauBackgroundWorker : HangfireBackgroundWorkerBase
{
    private readonly WordpressManagerLongChau _wordpressManagerLongChau;
    public SyncArticleLongChauBackgroundWorker(WordpressManagerLongChau wordpressManagerLongChau)
    {
        _wordpressManagerLongChau = wordpressManagerLongChau;
        RecurringJobId            = "Sync_Article_LongChau_BackgroundWorker";
        CronExpression            = Cron.Daily(GlobalConfig.Crawler.SyncTimeHours,0);
    }

    public override async Task DoWorkAsync()
    {
        await _wordpressManagerLongChau.DoSyncCategoriesAsync();
        await _wordpressManagerLongChau.DoSyncPostAsync();
    }
}

public class ReSyncArticleLongChauBackgroundWorker : HangfireBackgroundWorkerBase
{
    private readonly WordpressManagerLongChau _wordpressManagerLongChau;
    public ReSyncArticleLongChauBackgroundWorker(WordpressManagerLongChau wordpressManagerLongChau)
    {
        _wordpressManagerLongChau = wordpressManagerLongChau;
        RecurringJobId            = "ReSync_Article_LongChau_BackgroundWorker";
        CronExpression            = Cron.Daily(GlobalConfig.Crawler.ReSyncTimeHours,0);
    }

    public override async Task DoWorkAsync()
    {
        await _wordpressManagerLongChau.DoReSyncPostAsync();
    }
}