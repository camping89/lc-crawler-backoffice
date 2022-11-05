using System.Threading.Tasks;
using LC.Crawler.BackOffice.Configs;
using LC.Crawler.BackOffice.Medias;
using Volo.Abp.BackgroundWorkers.Hangfire;

namespace LC.Crawler.BackOffice.BackgroundWorkers.SucKhoeDoiSong;

public class DownloadMediaSucKhoeDoiSongBackgroundWorker : HangfireBackgroundWorkerBase
{
    private readonly MediaManagerSucKhoeDoiSong _mediaManager;

    public DownloadMediaSucKhoeDoiSongBackgroundWorker(MediaManagerSucKhoeDoiSong mediaManager)
    {
        _mediaManager = mediaManager;
        RecurringJobId            = "Download_Media_SucKhoeDoiSong_BackgroundWorker";
        CronExpression            = GlobalCronConsts.Every60Minutes;
        
    }

    public override async Task DoWorkAsync()
    {
        await _mediaManager.ProcessDownloadMediasAsync();
    }
}