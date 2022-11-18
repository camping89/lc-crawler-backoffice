using System.Threading.Tasks;
using Hangfire;
using LC.Crawler.BackOffice.Configs;
using LC.Crawler.BackOffice.Wordpress;
using Volo.Abp.BackgroundWorkers.Hangfire;
using WooCategory = WordPressPCL.Models.Category;

namespace LC.Crawler.BackOffice.BackgroundWorkers.SucKhoeGiaDinh;

public class SyncArticleSucKhoeGiaDinhBackgroundWorker : HangfireBackgroundWorkerBase
{
    private readonly WordpressManagerSucKhoeGiaDinh    _wordpressManagerSucKhoeGiaDinh;
    
    
    public SyncArticleSucKhoeGiaDinhBackgroundWorker(WordpressManagerSucKhoeGiaDinh wordpressManagerSucKhoeGiaDinh)
    {
        _wordpressManagerSucKhoeGiaDinh = wordpressManagerSucKhoeGiaDinh;
        
        RecurringJobId                  = "Sync_Article_SucKhoeGiaDinh_BackgroundWorker";
        CronExpression                  = Cron.Daily(GlobalConfig.Crawler.SyncTimeHours, 0);
    }

    public override async Task DoWorkAsync()
    {
        await _wordpressManagerSucKhoeGiaDinh.DoSyncCategoriesAsync();
        await _wordpressManagerSucKhoeGiaDinh.DoSyncPostAsync();
    }
}

public class ReSyncArticleSucKhoeGiaDinhBackgroundWorker : HangfireBackgroundWorkerBase
{
    private readonly WordpressManagerSucKhoeGiaDinh    _wordpressManagerSucKhoeGiaDinh;
    
    
    public ReSyncArticleSucKhoeGiaDinhBackgroundWorker(WordpressManagerSucKhoeGiaDinh wordpressManagerSucKhoeGiaDinh)
    {
        _wordpressManagerSucKhoeGiaDinh = wordpressManagerSucKhoeGiaDinh;
        
        RecurringJobId                  = "ReSync_Article_SucKhoeGiaDinh_BackgroundWorker";
        CronExpression                  = Cron.Daily(GlobalConfig.Crawler.ReSyncTimeHours, 0);
    }

    public override async Task DoWorkAsync()
    {
        await _wordpressManagerSucKhoeGiaDinh.DoReSyncPostAsync();
    }
}