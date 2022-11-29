using Hangfire;
using LC.Crawler.BackOffice.Configs;
using LC.Crawler.BackOffice.MessageQueue.Producers;
using Volo.Abp.BackgroundWorkers.Hangfire;

namespace LC.Crawler.BackOffice.MessageQueue.BackgroundWorkers;

public class PushDatasourceBackgroundWorker : HangfireBackgroundWorkerBase
{
    private readonly CrawlerDataSourceProducer _crawlerDataSourceProducer;

    public PushDatasourceBackgroundWorker(CrawlerDataSourceProducer crawlerDataSourceProducer)
    {
        _crawlerDataSourceProducer = crawlerDataSourceProducer;
        
        RecurringJobId            = "PushDatasource_BackgroundWorker";
        CronExpression            = Cron.Daily(17);
    }

    public override async Task DoWorkAsync()
    {
        await _crawlerDataSourceProducer.InitCrawlerDataSourceQueueAsync();
    }
}