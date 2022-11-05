using System.Threading.Tasks;
using LC.Crawler.BackOffice.Configs;
using LC.Crawler.BackOffice.Medias;
using Volo.Abp.BackgroundWorkers.Hangfire;

namespace LC.Crawler.BackOffice.BackgroundWorkers.AloBacSi;

public class DownloadMediaAloBacSiBackgroundWorker : HangfireBackgroundWorkerBase
{
    private readonly MediaManagerAloBacSi _mediaManager;

    public DownloadMediaAloBacSiBackgroundWorker(MediaManagerAloBacSi mediaManager)
    {
        _mediaManager = mediaManager;
        RecurringJobId            = "Download_Media_AloBacSi_BackgroundWorker";
        CronExpression            = GlobalCronConsts.Every60Minutes;
        
    }

    public override async Task DoWorkAsync()
    {
        await _mediaManager.ProcessDownloadMediasAsync();
    }
}