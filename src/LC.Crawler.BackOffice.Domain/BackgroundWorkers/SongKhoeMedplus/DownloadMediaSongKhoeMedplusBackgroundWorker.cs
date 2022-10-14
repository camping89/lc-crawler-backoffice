using System.Threading.Tasks;
using LC.Crawler.BackOffice.Configs;
using LC.Crawler.BackOffice.Medias;
using Volo.Abp.BackgroundWorkers.Hangfire;

namespace LC.Crawler.BackOffice.BackgroundWorkers.SongKhoeMedplus;

public class DownloadMediaSongKhoeMedplusBackgroundWorker: HangfireBackgroundWorkerBase
{
    private readonly MediaManagerSongKhoeMedplus _mediaManager;

    public DownloadMediaSongKhoeMedplusBackgroundWorker(MediaManagerSongKhoeMedplus mediaManager)
    {
        _mediaManager = mediaManager;
        RecurringJobId            = nameof(DownloadMediaSongKhoeMedplusBackgroundWorker);
        CronExpression            = GlobalCronConsts.Every60Minutes;
    }
    
    public override async Task DoWorkAsync()
    {
        await _mediaManager.ProcessDownloadMediasAsync();
    }
}