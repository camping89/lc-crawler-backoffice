using System.Threading.Tasks;
using LC.Crawler.BackOffice.Configs;
using LC.Crawler.BackOffice.Medias;
using Volo.Abp.BackgroundWorkers.Hangfire;

namespace LC.Crawler.BackOffice.BackgroundWorkers.Aladin;

public class DownloadMediaAladinBackgroundWorker : HangfireBackgroundWorkerBase
{
    private readonly MediaManagerAladin _mediaManager;

    public DownloadMediaAladinBackgroundWorker(MediaManagerAladin mediaManager)
    {
        _mediaManager = mediaManager;
        RecurringJobId            = "Download_Media_Aladin_BackgroundWorker";
        CronExpression            = GlobalCronConsts.Every60Minutes;
        
    }

    public override async Task DoWorkAsync()
    {
        await _mediaManager.ProcessDownloadMediasAsync();
    }
}