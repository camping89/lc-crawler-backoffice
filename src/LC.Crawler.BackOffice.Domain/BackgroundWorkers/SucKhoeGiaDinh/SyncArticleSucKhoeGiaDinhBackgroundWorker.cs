using System.Threading.Tasks;
using Hangfire;
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
        
        RecurringJobId                  = nameof(SyncArticleSucKhoeGiaDinhBackgroundWorker);
        CronExpression                  = Cron.Daily(0, 0);
    }

    public override async Task DoWorkAsync()
    {
        await _wordpressManagerSucKhoeGiaDinh.DoSyncCategoriesAsync();
        await _wordpressManagerSucKhoeGiaDinh.DoSyncPostAsync();
    }
}