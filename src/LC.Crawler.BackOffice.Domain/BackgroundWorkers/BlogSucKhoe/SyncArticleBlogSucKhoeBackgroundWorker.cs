using System.Threading.Tasks;
using Hangfire;
using LC.Crawler.BackOffice.Configs;
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

        RecurringJobId            = "Sync_Article_BlogSucKhoe_BackgroundWorker";
        CronExpression            = Cron.Daily(GlobalConfig.Crawler.SyncTimeHours,0);
    }

    public override async Task DoWorkAsync()
    {
        await _wordpressManagerBlogSucKhoe.DoSyncCategoriesAsync();
        await _wordpressManagerBlogSucKhoe.DoSyncPostAsync();
    }
}

public class ReSyncArticleBlogSucKhoeBackgroundWorker : HangfireBackgroundWorkerBase
{
    private readonly WordpressManagerBlogSucKhoe _wordpressManagerBlogSucKhoe;
    
    public ReSyncArticleBlogSucKhoeBackgroundWorker(WordpressManagerBlogSucKhoe wordpressManagerBlogSucKhoe)
    {
        _wordpressManagerBlogSucKhoe = wordpressManagerBlogSucKhoe;

        RecurringJobId            = "ReSync_Article_BlogSucKhoe_BackgroundWorker";
        CronExpression            = Cron.Daily(GlobalConfig.Crawler.ReSyncTimeHours,0);
    }

    public override async Task DoWorkAsync()
    {
        await _wordpressManagerBlogSucKhoe.DoReSyncPostAsync();
    }
}