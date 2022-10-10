using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using LC.Crawler.BackOffice.Localization;
using LC.Crawler.BackOffice.MultiTenancy;
using Volo.Abp.AuditLogging;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.BlobStoring;
using Volo.Abp.Emailing;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Identity;
using Volo.Abp.IdentityServer;
using Volo.Abp.LanguageManagement;
using Volo.Abp.LeptonTheme.Management;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.PermissionManagement.Identity;
using Volo.Abp.PermissionManagement.IdentityServer;
using Volo.Abp.SettingManagement;
using Volo.Abp.TextTemplateManagement;
using Volo.Saas;
using Volo.Abp.BlobStoring.Database;
using Volo.Abp.Commercial.SuiteTemplates;
using Volo.Abp.Gdpr;
using Volo.FileManagement;
using Volo.Abp.BlobStoring.FileSystem;
using Volo.Abp.BackgroundWorkers.Hangfire;

namespace LC.Crawler.BackOffice;

[DependsOn(
    typeof(BackOfficeDomainSharedModule),
    typeof(AbpAuditLoggingDomainModule),
    typeof(AbpBackgroundJobsDomainModule),
    typeof(AbpFeatureManagementDomainModule),
    typeof(AbpIdentityProDomainModule),
    typeof(AbpPermissionManagementDomainIdentityModule),
    typeof(AbpIdentityServerDomainModule),
    typeof(AbpPermissionManagementDomainIdentityServerModule),
    typeof(AbpSettingManagementDomainModule),
    typeof(SaasDomainModule),
    typeof(TextTemplateManagementDomainModule),
    typeof(LeptonThemeManagementDomainModule),
    typeof(LanguageManagementDomainModule),
    typeof(VoloAbpCommercialSuiteTemplatesModule),
    typeof(AbpEmailingModule),
    typeof(AbpGdprDomainModule),
    typeof(BlobStoringDatabaseDomainModule)
    )]
    [DependsOn(typeof(FileManagementDomainModule))]
    [DependsOn(typeof(AbpBlobStoringFileSystemModule))]
    public class BackOfficeDomainModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        Configure<AbpMultiTenancyOptions>(options =>
        {
            options.IsEnabled = MultiTenancyConsts.IsEnabled;
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Languages.Add(new LanguageInfo("en", "en", "English", "gb"));
        });

        
        Configure<AbpBlobStoringOptions>(options =>
        {
            options.Containers.ConfigureDefault(container =>
            {
                container.UseFileSystem(fileSystem =>
                {
                    fileSystem.BasePath = configuration["MediaPath"];
                });
            });
        });

#if DEBUG
        context.Services.Replace(ServiceDescriptor.Singleton<IEmailSender, NullEmailSender>());
#endif
    }
}
