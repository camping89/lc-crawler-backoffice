using System.Threading.Tasks;
using LC.Crawler.BackOffice.Configs;
using LC.Crawler.BackOffice.Medias;
using Volo.Abp.BackgroundWorkers.Hangfire;

namespace LC.Crawler.BackOffice.BackgroundWorkers.SucKhoeGiaDinh;

public class DownloadMediaSucKhoeGiaDinhBackgroundWorker : HangfireBackgroundWorkerBase
{
    private readonly MediaManagerSucKhoeGiaDinh _mediaManager;

    public DownloadMediaSucKhoeGiaDinhBackgroundWorker(MediaManagerSucKhoeGiaDinh mediaManager)
    {
        _mediaManager  = mediaManager;
        RecurringJobId = "Download_Media_SucKhoeGiaDinh_BackgroundWorker";
        CronExpression = GlobalCronConsts.Every60Minutes;
    }

    public override async Task DoWorkAsync()
    {
        await _mediaManager.ProcessDownloadMediasAsync();
    }
}