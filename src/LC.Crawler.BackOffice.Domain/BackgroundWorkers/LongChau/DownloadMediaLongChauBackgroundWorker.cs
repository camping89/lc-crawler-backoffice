using System.Threading.Tasks;
using LC.Crawler.BackOffice.Configs;
using LC.Crawler.BackOffice.Medias;
using Volo.Abp.BackgroundWorkers.Hangfire;

namespace LC.Crawler.BackOffice.BackgroundWorkers.LongChau;

public class DownloadMediaLongChauBackgroundWorker : HangfireBackgroundWorkerBase
{
    private readonly MediaManager _mediaManager;

    public DownloadMediaLongChauBackgroundWorker(MediaManager mediaManager)
    {
        _mediaManager = mediaManager;
        RecurringJobId            = nameof(DownloadMediaLongChauBackgroundWorker);
        CronExpression            = GlobalCronConsts.Every60Minutes;
        
    }

    public override async Task DoWorkAsync()
    {
        await _mediaManager.ProcessDownloadMediasAsync();
    }
}