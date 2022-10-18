using System.Threading.Tasks;
using Hangfire;
using LC.Crawler.BackOffice.Wordpress;
using Volo.Abp.BackgroundWorkers.Hangfire;
using WooCategory = WordPressPCL.Models.Category;

namespace LC.Crawler.BackOffice.BackgroundWorkers.BlogSucKhoe;

public class SyncArticleBlogSucKhoeBackgroundWorker : HangfireBackgroundWorkerBase
{
    private readonly WordpressManagerBlogSucKhoe _wordpressManagerBlogSucKhoe;
    
    public SyncArticleBlogSucKhoeBackgroundWorker(WordpressManagerBlogSucKhoe wordpressManagerBlogSucKhoe)
    {
        _wordpressManagerBlogSucKhoe = wordpressManagerBlogSucKhoe;

        RecurringJobId            = nameof(SyncArticleBlogSucKhoeBackgroundWorker);
        CronExpression            = Cron.Daily(0,0);
    }

    public override async Task DoWorkAsync()
    {
        await _wordpressManagerBlogSucKhoe.DoSyncToWordpress();
    }
}