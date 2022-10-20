using System.Threading.Tasks;
using Hangfire;
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
        
        RecurringJobId                   = nameof(SyncArticleSongKhoeMedplusBackgroundWorker);
        CronExpression                   = Cron.Daily(0, 0);
    }
    
    public override async Task DoWorkAsync()
    {
        await _wordpressManagerSongKhoeMedplus.DoSyncPostAsync();
    }
}