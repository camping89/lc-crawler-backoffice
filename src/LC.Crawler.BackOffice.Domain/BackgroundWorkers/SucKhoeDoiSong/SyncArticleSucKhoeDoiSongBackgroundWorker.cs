using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using HtmlAgilityPack;
using LC.Crawler.BackOffice.Articles;
using LC.Crawler.BackOffice.Categories;
using LC.Crawler.BackOffice.DataSources;
using LC.Crawler.BackOffice.Medias;
using LC.Crawler.BackOffice.Wordpress;
using Volo.Abp.BackgroundWorkers.Hangfire;
using WordPressPCL;
using WordPressPCL.Models;
using WooCategory = WordPressPCL.Models.Category;
using Guid = System.Guid;

namespace LC.Crawler.BackOffice.BackgroundWorkers.SucKhoeDoiSong;

public class SyncArticleSucKhoeDoiSongBackgroundWorker : HangfireBackgroundWorkerBase
{
    private readonly WordpressManagerSucKhoeDoiSong _wordpressManagerSucKhoeDoiSong;
    public SyncArticleSucKhoeDoiSongBackgroundWorker(WordpressManagerSucKhoeDoiSong wordpressManagerSucKhoeDoiSong)
    {
        _wordpressManagerSucKhoeDoiSong = wordpressManagerSucKhoeDoiSong;
        
        RecurringJobId                  = nameof(SyncArticleSucKhoeDoiSongBackgroundWorker);
        CronExpression                  = Cron.Daily(0, 0);
    }

    public override async Task DoWorkAsync()
    {
        await _wordpressManagerSucKhoeDoiSong.DoSyncPostAsync();
    }
}