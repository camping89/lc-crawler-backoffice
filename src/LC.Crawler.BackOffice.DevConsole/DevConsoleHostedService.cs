using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Volo.Abp;

namespace LC.Crawler.BackOffice.DevConsole;

public class DevConsoleHostedService : IHostedService
{
    private readonly IAbpApplicationWithExternalServiceProvider _abpApplication;
    private readonly MasterService _masterService;
    private readonly ImageService _imageService;
    public DevConsoleHostedService(MasterService masterService, IAbpApplicationWithExternalServiceProvider abpApplication, ImageService imageService)
    {
        _masterService = masterService;
        _abpApplication = abpApplication;
        _imageService = imageService;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        //await _masterService.ResetLongChau();
        //await _masterService.ProcessLongChauDataAsync();
        //await _masterService.DownLoadMediaLongChauAsync();
        //await _masterService.DoSyncProductToWooAsync();

        //await _masterService.ProcessLongChauArticleDataAsync();
        //await _masterService.DoSyncArticles();
        //await _masterService.DoResyncAladinProduct();

        //await _masterService.DoSyncProductToWooAsync();
        //await _masterService.CleanPostDuplicate();
        //await _masterService.DoSyncProductToWooAsync();

        //await _imageService.UpdateUrl();
        await _masterService.SyncAllPosts();
        //await _masterService.DeletePost();

        //await _masterService.ProcessAloBacSiDataAsync();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _abpApplication.ShutdownAsync();
    }
}
