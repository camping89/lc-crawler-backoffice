using System.Threading.Tasks;
using Hangfire;
using LC.Crawler.BackOffice.Configs;
using LC.Crawler.BackOffice.Wordpress;
using Volo.Abp.BackgroundWorkers.Hangfire;
using WooCategory = WordPressPCL.Models.Category;

namespace LC.Crawler.BackOffice.BackgroundWorkers.SongKhoeMedplus;

public class SyncArticleSongKhoeMedplusBackgroundWorker : HangfireBackgroundWorkerBase
{
    private readonly WordpressManagerSongKhoeMedplus _wordpressManagerSongKhoeMedplus;

    public SyncArticleSongKhoeMedplusBackgroundWorker(WordpressManagerSongKhoeMedplus wordpressManagerSongKhoeMedplus)
    {
        _wordpressManagerSongKhoeMedplus = wordpressManagerSongKhoeMedplus;
        
        RecurringJobId                   = "Sync_Article_SongKhoeMedplus_BackgroundWorker";
        CronExpression                   = Cron.Daily(GlobalConfig.Crawler.SyncTimeHours, 0);
    }
    
    public override async Task DoWorkAsync()
    {
        await _wordpressManagerSongKhoeMedplus.DoSyncCategoriesAsync();
        await _wordpressManagerSongKhoeMedplus.DoSyncPostAsync();
    }
}

public class ReSyncArticleSongKhoeMedplusBackgroundWorker : HangfireBackgroundWorkerBase
{
    private readonly WordpressManagerSongKhoeMedplus _wordpressManagerSongKhoeMedplus;

    public ReSyncArticleSongKhoeMedplusBackgroundWorker(WordpressManagerSongKhoeMedplus wordpressManagerSongKhoeMedplus)
    {
        _wordpressManagerSongKhoeMedplus = wordpressManagerSongKhoeMedplus;
        
        RecurringJobId                   = "ReSync_Article_SongKhoeMedplus_BackgroundWorker";
        CronExpression                   = Cron.Daily(GlobalConfig.Crawler.ReSyncTimeHours, 0);
    }
    
    public override async Task DoWorkAsync()
    {
        await _wordpressManagerSongKhoeMedplus.DoReSyncPostAsync();
    }
}