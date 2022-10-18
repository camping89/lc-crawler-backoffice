using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using HtmlAgilityPack;
using LC.Crawler.BackOffice.Articles;
using LC.Crawler.BackOffice.DataSources;
using LC.Crawler.BackOffice.Medias;
using LC.Crawler.BackOffice.Wordpress;
using Volo.Abp.BackgroundWorkers.Hangfire;
using WordPressPCL;
using WordPressPCL.Models;

namespace LC.Crawler.BackOffice.BackgroundWorkers.SieuThiSongKhoe;

public class SyncArticleSieuThiSongKhoeBackgroundWorker : HangfireBackgroundWorkerBase
{
    private readonly WordpressManagerSieuThiSongKhoe _wordpressManagerSieuThiSongKhoe;
    public SyncArticleSieuThiSongKhoeBackgroundWorker(WordpressManagerSieuThiSongKhoe wordpressManagerSieuThiSongKhoe)
    {
        _wordpressManagerSieuThiSongKhoe = wordpressManagerSieuThiSongKhoe;
        
        RecurringJobId                   = nameof(SyncArticleSieuThiSongKhoeBackgroundWorker);
        CronExpression                   = Cron.Daily(0, 0);
    }

    public override async Task DoWorkAsync()
    {
        await _wordpressManagerSieuThiSongKhoe.DoSyncToWordpress();
    }
}