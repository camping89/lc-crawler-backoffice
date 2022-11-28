using System;
using Mindscape.Raygun4Net.AspNetCore;
using Volo.Abp.Domain.Services;

namespace LC.Crawler.BackOffice.Logs;

public class RayGunExceptionReport : DomainService
{
    // Todoo: Config Raygun in global config
    private readonly RaygunClient _client = new RaygunClient("4sz6j5dIfLo66ftAH1Pfg");
    public void LogException(Exception ex, string description = "")
    {
        var date = DateTime.UtcNow.Date.ToString("dd-MM-yyyy");
        // Todoo: Classify Exception (performance, critical, fatal, ...)
        var exc = new Exception($"{ex}_{description}_{date}");
        _client.Send(exc);
    }
}