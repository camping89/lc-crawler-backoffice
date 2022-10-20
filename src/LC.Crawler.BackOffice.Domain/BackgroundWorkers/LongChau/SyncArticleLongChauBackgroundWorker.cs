using System.Threading.Tasks;
using Hangfire;
using LC.Crawler.BackOffice.Wordpress;
using Volo.Abp.BackgroundWorkers.Hangfire;

namespace LC.Crawler.BackOffice.BackgroundWorkers.LongChau;

public class SyncArticleLongChauBackgroundWorker : HangfireBackgroundWorkerBase
{
    private readonly WordpressManagerLongChau _wordpressManagerLongChau;
    public SyncArticleLongChauBackgroundWorker(WordpressManagerLongChau wordpressManagerLongChau)
    {
        _wordpressManagerLongChau = wordpressManagerLongChau;
        RecurringJobId            = nameof(SyncArticleLongChauBackgroundWorker);
        CronExpression            = Cron.Daily(0,0);
    }

    public override async Task DoWorkAsync()
    {
        await _wordpressManagerLongChau.DoSyncPostAsync();
    }
}