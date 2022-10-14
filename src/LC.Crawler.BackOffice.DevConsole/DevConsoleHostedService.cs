using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Volo.Abp;

namespace LC.Crawler.BackOffice.DevConsole;

public class DevConsoleHostedService : IHostedService
{
    private readonly IAbpApplicationWithExternalServiceProvider _abpApplication;
    private readonly MasterService _masterService;

    public DevConsoleHostedService(MasterService masterService, IAbpApplicationWithExternalServiceProvider abpApplication)
    {
        _masterService = masterService;
        _abpApplication = abpApplication;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _masterService.ProcessLongChauDataAsync();
        //await _masterService.DownLoadMediaLongChauAsync();
        //await _masterService.DoSyncProductToWooAsync();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _abpApplication.ShutdownAsync();
    }
}
