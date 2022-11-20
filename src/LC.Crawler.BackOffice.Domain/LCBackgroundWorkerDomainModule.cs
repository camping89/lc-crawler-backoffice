using System;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.SqlServer;
using LC.Crawler.BackOffice.BackgroundWorkers;
using LC.Crawler.BackOffice.BackgroundWorkers.Aladin;
using LC.Crawler.BackOffice.BackgroundWorkers.AloBacSi;
using LC.Crawler.BackOffice.BackgroundWorkers.BlogSucKhoe;
using LC.Crawler.BackOffice.BackgroundWorkers.LongChau;
using LC.Crawler.BackOffice.BackgroundWorkers.SongKhoeMedplus;
using LC.Crawler.BackOffice.BackgroundWorkers.SieuThiSongKhoe;
using LC.Crawler.BackOffice.BackgroundWorkers.SucKhoeDoiSong;
using LC.Crawler.BackOffice.BackgroundWorkers.SucKhoeGiaDinh;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Volo.Abp;
using Volo.Abp.Modularity;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.BackgroundWorkers.Hangfire;

namespace LC.Crawler.BackOffice;
[DependsOn(typeof(AbpBackgroundWorkersHangfireModule))]
public class LCBackgroundWorkerDomainModule : AbpModule
{
    public override Task OnApplicationInitializationAsync(
        ApplicationInitializationContext context)
    {
        ConfigBackgroundWorker(context);
        return Task.CompletedTask;
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    { 
        var configuration      = context.Services.GetConfiguration();
        ConfigureHangfire(context, configuration);
    }
    
    private Task ConfigBackgroundWorker(ApplicationInitializationContext context)
    {
        var hostEnvironment = context.ServiceProvider.GetRequiredService<IHostEnvironment>();

        if (true)
        {
            // Process download and save image
            //context.AddBackgroundWorkerAsync<DownloadMediaLongChauBackgroundWorker>();
            //context.AddBackgroundWorkerAsync<ParserArticleLongChauBackgroundWorker>();
            context.AddBackgroundWorkerAsync<SyncArticleLongChauBackgroundWorker>();
            context.AddBackgroundWorkerAsync<ReSyncArticleLongChauBackgroundWorker>();
            context.AddBackgroundWorkerAsync<SyncProductLongChauBackgroundWorker>();
            context.AddBackgroundWorkerAsync<ReSyncProductLongChauBackgroundWorker>();
            //
            //Aladin
            //context.AddBackgroundWorkerAsync<DownloadMediaAladinBackgroundWorker>();
            context.AddBackgroundWorkerAsync<SyncProductAladinBackgroundWorker>();
            context.AddBackgroundWorkerAsync<ReSyncProductAladinBackgroundWorker>();
            context.AddBackgroundWorkerAsync<SyncArticleAladinBackgroundWorker>();
            context.AddBackgroundWorkerAsync<ReSyncArticleAladinBackgroundWorker>();
            
            //Suckhoedoisong
            //context.AddBackgroundWorkerAsync<DownloadMediaSucKhoeDoiSongBackgroundWorker>();
            context.AddBackgroundWorkerAsync<SyncArticleSucKhoeDoiSongBackgroundWorker>();
            context.AddBackgroundWorkerAsync<ReSyncArticleSucKhoeDoiSongBackgroundWorker>();
            
            //SieuThiSongKhoe
            //context.AddBackgroundWorkerAsync<DownloadMediaSieuThiSongKhoeBackgroundWorker>();
            context.AddBackgroundWorkerAsync<SyncProductSieuThiSongKhoeBackgroundWorker>();
            context.AddBackgroundWorkerAsync<ReSyncProductSieuThiSongKhoeBackgroundWorker>();
            context.AddBackgroundWorkerAsync<SyncArticleSieuThiSongKhoeBackgroundWorker>();
            context.AddBackgroundWorkerAsync<ReSyncArticleSieuThiSongKhoeBackgroundWorker>();
            
            //SongKhoeMedplus
            //context.AddBackgroundWorkerAsync<DownloadMediaSongKhoeMedplusBackgroundWorker>();
            context.AddBackgroundWorkerAsync<SyncArticleSongKhoeMedplusBackgroundWorker>();
            context.AddBackgroundWorkerAsync<ReSyncArticleSongKhoeMedplusBackgroundWorker>();
            
            // Blog Suc Khoe
            //context.AddBackgroundWorkerAsync<DownloadMediaBlogSucKhoeBackgroundWorker>();
            context.AddBackgroundWorkerAsync<SyncArticleBlogSucKhoeBackgroundWorker>();
            context.AddBackgroundWorkerAsync<ReSyncArticleBlogSucKhoeBackgroundWorker>();
            
            // Suc Khoe Gia Dinh
            //context.AddBackgroundWorkerAsync<DownloadMediaSucKhoeGiaDinhBackgroundWorker>();
            context.AddBackgroundWorkerAsync<SyncArticleSucKhoeGiaDinhBackgroundWorker>();
            context.AddBackgroundWorkerAsync<ReSyncArticleSucKhoeGiaDinhBackgroundWorker>();
            
            // Alo Bac Si
            //context.AddBackgroundWorkerAsync<DownloadMediaAloBacSiBackgroundWorker>();
            context.AddBackgroundWorkerAsync<SyncArticleAloBacSiBackgroundWorker>();
            context.AddBackgroundWorkerAsync<ReSyncArticleAloBacSiBackgroundWorker>();
        }
        else
        {
            // Process download and save image
            //context.AddBackgroundWorkerAsync<DownloadMediaLongChauBackgroundWorker>();
            //context.AddBackgroundWorkerAsync<ParserArticleLongChauBackgroundWorker>();
            context.AddBackgroundWorkerAsync<SyncArticleLongChauBackgroundWorker>();
            context.AddBackgroundWorkerAsync<ReSyncArticleLongChauBackgroundWorker>();
            context.AddBackgroundWorkerAsync<SyncProductLongChauBackgroundWorker>();
            context.AddBackgroundWorkerAsync<ReSyncProductLongChauBackgroundWorker>();
            
            //Aladin
            //context.AddBackgroundWorkerAsync<DownloadMediaAladinBackgroundWorker>();
            context.AddBackgroundWorkerAsync<SyncProductAladinBackgroundWorker>();
            context.AddBackgroundWorkerAsync<ReSyncProductAladinBackgroundWorker>();
            
            //SieuThiSongKhoe
            //context.AddBackgroundWorkerAsync<DownloadMediaSieuThiSongKhoeBackgroundWorker>();
            context.AddBackgroundWorkerAsync<SyncProductSieuThiSongKhoeBackgroundWorker>();
            context.AddBackgroundWorkerAsync<ReSyncProductSieuThiSongKhoeBackgroundWorker>();
            context.AddBackgroundWorkerAsync<SyncArticleSieuThiSongKhoeBackgroundWorker>();
            context.AddBackgroundWorkerAsync<ReSyncArticleSieuThiSongKhoeBackgroundWorker>();
            
            //Suckhoedoisong
            //context.AddBackgroundWorkerAsync<DownloadMediaSucKhoeDoiSongBackgroundWorker>();
            context.AddBackgroundWorkerAsync<SyncArticleSucKhoeDoiSongBackgroundWorker>();
            context.AddBackgroundWorkerAsync<ReSyncArticleSucKhoeDoiSongBackgroundWorker>();
            
            //SongKhoeMedplus
            //context.AddBackgroundWorkerAsync<DownloadMediaSongKhoeMedplusBackgroundWorker>();
            context.AddBackgroundWorkerAsync<SyncArticleSongKhoeMedplusBackgroundWorker>();
            context.AddBackgroundWorkerAsync<ReSyncArticleSongKhoeMedplusBackgroundWorker>();
            
            // Blog Suc Khoe
            //context.AddBackgroundWorkerAsync<DownloadMediaBlogSucKhoeBackgroundWorker>();
            context.AddBackgroundWorkerAsync<SyncArticleBlogSucKhoeBackgroundWorker>();
            context.AddBackgroundWorkerAsync<ReSyncArticleBlogSucKhoeBackgroundWorker>();
            
            // Suc Khoe Gia Dinh
            //context.AddBackgroundWorkerAsync<DownloadMediaSucKhoeGiaDinhBackgroundWorker>();
            context.AddBackgroundWorkerAsync<SyncArticleSucKhoeGiaDinhBackgroundWorker>();
            context.AddBackgroundWorkerAsync<ReSyncArticleSucKhoeGiaDinhBackgroundWorker>();
            
            // Alo Bac Si
            //context.AddBackgroundWorkerAsync<DownloadMediaAloBacSiBackgroundWorker>();
            context.AddBackgroundWorkerAsync<SyncArticleAloBacSiBackgroundWorker>();
            context.AddBackgroundWorkerAsync<ReSyncArticleAloBacSiBackgroundWorker>();
        }
        
        return Task.CompletedTask;
    }
    
    private void ConfigureHangfire(ServiceConfigurationContext context, IConfiguration configuration)
    {
        // hangfire - disable retry
        GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute { Attempts = 0 });

            
        // JobStorage.Current = new SqlServerStorage(configuration.GetConnectionString("Hangfire"));
        context.Services.AddHangfire(config =>
        {
            config.UseSqlServerStorage(configuration.GetConnectionString("Hangfire"),
                new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout       = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout   = TimeSpan.FromMinutes(5),
                    QueuePollInterval            = TimeSpan.Zero,
                    UseRecommendedIsolationLevel = true,
                    DisableGlobalLocks           = true,
                    SchemaName                   = "LCCrawlerJob"
                });
            // hangfire - disable retry
            // use GlobalJobFilters above
            // config.UseFilter(new AutomaticRetryAttribute { Attempts = 0 });
        });
    }
}