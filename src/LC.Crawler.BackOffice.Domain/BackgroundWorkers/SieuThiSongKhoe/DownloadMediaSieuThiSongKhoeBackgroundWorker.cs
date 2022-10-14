using System.Threading.Tasks;
using LC.Crawler.BackOffice.Configs;
using LC.Crawler.BackOffice.Medias;
using Volo.Abp.BackgroundWorkers.Hangfire;

namespace LC.Crawler.BackOffice.BackgroundWorkers.SieuThiSongKhoe;

public class DownloadMediaSieuThiSongKhoeBackgroundWorker : HangfireBackgroundWorkerBase
{
    private readonly MediaManagerSieuThiSongKhoe _mediaManager;

    public DownloadMediaSieuThiSongKhoeBackgroundWorker(MediaManagerSieuThiSongKhoe mediaManager)
    {
        _mediaManager = mediaManager;
        RecurringJobId            = nameof(DownloadMediaSieuThiSongKhoeBackgroundWorker);
        CronExpression            = GlobalCronConsts.Every60Minutes;
    }
    
    public override async Task DoWorkAsync()
    {
        await _mediaManager.ProcessDownloadMediasAsync();
    }
}