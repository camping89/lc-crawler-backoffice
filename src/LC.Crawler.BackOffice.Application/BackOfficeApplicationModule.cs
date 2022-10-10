using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Volo.Abp.Account;
using Volo.Abp.AuditLogging;
using Volo.Abp.AutoMapper;
using Volo.Abp.BlobStoring;
using Volo.Abp.BlobStoring.Database;
using Volo.Abp.Emailing;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Gdpr;
using Volo.Abp.Identity;
using Volo.Abp.IdentityServer;
using Volo.Abp.LanguageManagement;
using Volo.Abp.LeptonTheme.Management;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.TextTemplateManagement;
using Volo.Saas.Host;
using Volo.FileManagement;

namespace LC.Crawler.BackOffice;

[DependsOn(
    typeof(BackOfficeDomainModule),
    typeof(BackOfficeApplicationContractsModule),
    typeof(AbpIdentityApplicationModule),
    typeof(AbpPermissionManagementApplicationModule),
    typeof(AbpFeatureManagementApplicationModule),
    typeof(AbpSettingManagementApplicationModule),
    typeof(SaasHostApplicationModule),
    typeof(AbpAuditLoggingApplicationModule),
    typeof(AbpIdentityServerApplicationModule),
    typeof(AbpAccountPublicApplicationModule),
    typeof(AbpAccountAdminApplicationModule),
    typeof(LanguageManagementApplicationModule),
    typeof(LeptonThemeManagementApplicationModule),
    typeof(AbpGdprApplicationModule),
    typeof(TextTemplateManagementApplicationModule)
    )]
[DependsOn(typeof(FileManagementApplicationModule))]
    public class BackOfficeApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<BackOfficeApplicationModule>();
        });
    }
}
