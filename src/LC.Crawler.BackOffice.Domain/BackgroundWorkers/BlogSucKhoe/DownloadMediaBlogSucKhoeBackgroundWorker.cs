using System.Threading.Tasks;
using LC.Crawler.BackOffice.Configs;
using LC.Crawler.BackOffice.Medias;
using Volo.Abp.BackgroundWorkers.Hangfire;

namespace LC.Crawler.BackOffice.BackgroundWorkers.BlogSucKhoe;

public class DownloadMediaBlogSucKhoeBackgroundWorker : HangfireBackgroundWorkerBase
{
    private readonly MediaManagerBlogSucKhoe _mediaManager;

    public DownloadMediaBlogSucKhoeBackgroundWorker(MediaManagerBlogSucKhoe mediaManager)
    {
        _mediaManager  = mediaManager;
        RecurringJobId = "Download_Media_BlogSucKhoe_BackgroundWorker";
        CronExpression = GlobalCronConsts.Every60Minutes;
    }

    public override async Task DoWorkAsync()
    {
        await _mediaManager.ProcessDownloadMediasAsync();
    }
}