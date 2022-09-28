using Volo.Abp.Ui.Branding;
using Volo.Abp.DependencyInjection;

namespace LC.Crawler.BackOffice.Web;

[Dependency(ReplaceServices = true)]
public class BackOfficeBrandingProvider : DefaultBrandingProvider
{
    public override string AppName => "BackOffice";
}
