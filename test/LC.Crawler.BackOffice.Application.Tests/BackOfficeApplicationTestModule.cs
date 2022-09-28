using Volo.Abp.Modularity;

namespace LC.Crawler.BackOffice;

[DependsOn(
    typeof(BackOfficeApplicationModule),
    typeof(BackOfficeDomainTestModule)
    )]
public class BackOfficeApplicationTestModule : AbpModule
{

}
