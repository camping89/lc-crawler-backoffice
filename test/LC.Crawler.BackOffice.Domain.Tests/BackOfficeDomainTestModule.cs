using LC.Crawler.BackOffice.MongoDB;
using Volo.Abp.Modularity;

namespace LC.Crawler.BackOffice;

[DependsOn(
    typeof(BackOfficeMongoDbTestModule)
    )]
public class BackOfficeDomainTestModule : AbpModule
{

}
