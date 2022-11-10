using System.IO;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using LC.Crawler.BackOffice.MongoDB;
using LC.Crawler.BackOffice.Localization;
using LC.Crawler.BackOffice.MultiTenancy;
using LC.Crawler.BackOffice.Permissions;
using LC.Crawler.BackOffice.Web.Menus;
using Microsoft.OpenApi.Models;
using Volo.Abp;
using Volo.Abp.Account.Admin.Web;
using Volo.Abp.Account.Public.Web;
using Volo.Abp.Account.Public.Web.ExternalProviders;
using Volo.Abp.Account.Web;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc.Localization;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Lepton;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Shared;
using Volo.Abp.AuditLogging.Web;
using Volo.Abp.Autofac;
using Volo.Abp.AutoMapper;
using Volo.Abp.Identity.Web;
using Volo.Abp.IdentityServer.Web;
using Volo.Abp.LanguageManagement;
using Volo.Abp.LeptonTheme.Management;
using Volo.Abp.Modularity;
using Volo.Abp.TextTemplateManagement.Web;
using Volo.Abp.UI.Navigation.Urls;
using Volo.Abp.UI.Navigation;
using Volo.Abp.VirtualFileSystem;
using Volo.Saas.Host;
using System;
using System.Collections.Generic;
using Hangfire;
using HangfireBasicAuthenticationFilter;
using LC.Crawler.BackOffice.MessageQueue;
using LC.Crawler.BackOffice.PageDatasource;
using LC.Crawler.BackOffice.PageDatasource.Aladin.MongoDb;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Authentication.Twitter;
using Microsoft.AspNetCore.Extensions.DependencyInjection;
using LC.Crawler.BackOffice.Web.HealthChecks;
using Volo.Abp.AspNetCore.Mvc.UI.Bundling;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Lepton.Bundling;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Shared.Toolbars;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.Identity;
using Volo.Abp.Swashbuckle;
using Volo.Abp.Gdpr.Web;
using Volo.Abp.EventBus.RabbitMq;
using Volo.FileManagement.Web;

namespace LC.Crawler.BackOffice.Web;

[DependsOn(typeof(BackOfficeHttpApiModule),
    typeof(BackOfficeApplicationModule),
    typeof(BackOfficeMongoDbModule),
    typeof(PageDataSourceMongoDbModule),
    typeof(AbpAutofacModule),
    typeof(AbpIdentityWebModule),
    typeof(AbpAccountPublicWebIdentityServerModule),
    typeof(AbpAuditLoggingWebModule),
    typeof(LeptonThemeManagementWebModule),
    typeof(SaasHostWebModule),
    typeof(AbpAccountAdminWebModule),
    typeof(AbpIdentityServerWebModule),
    typeof(LanguageManagementWebModule),
    typeof(AbpAspNetCoreMvcUiLeptonThemeModule),
    typeof(TextTemplateManagementWebModule),
    typeof(AbpGdprWebModule),
    typeof(AbpSwashbuckleModule),
    typeof(AbpAspNetCoreSerilogModule))]
[DependsOn(
    //typeof(AbpEventBusRabbitMqModule),
    typeof(LCMessageQueueModule),
    typeof(LCBackgroundWorkerDomainModule))]
[DependsOn(typeof(FileManagementWebModule))]
public class BackOfficeWebModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.PreConfigure<AbpMvcDataAnnotationsLocalizationOptions>(options =>
        {
            options.AddAssemblyResource(typeof(BackOfficeResource),
                typeof(BackOfficeDomainModule).Assembly,
                typeof(BackOfficeDomainSharedModule).Assembly,
                typeof(BackOfficeApplicationModule).Assembly,
                typeof(BackOfficeApplicationContractsModule).Assembly,
                typeof(BackOfficeWebModule).Assembly);
        });
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetHostingEnvironment();
        var configuration = context.Services.GetConfiguration();

        ConfigureBundles();
        ConfigureUrls(configuration);
        ConfigurePages(configuration);
        ConfigureAuthentication(context, configuration);
        ConfigureImpersonation(context, configuration);
        ConfigureAutoMapper();
        ConfigureVirtualFileSystem(hostingEnvironment);
        ConfigureNavigationServices();
        ConfigureAutoApiControllers();
        ConfigureSwaggerServices(context, configuration);
        ConfigureExternalProviders(context);
        ConfigureHealthChecks(context);
    }

    private void ConfigureHealthChecks(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetHostingEnvironment();
        if (!hostingEnvironment.IsDevelopment())
        {
            context.Services.AddBackOfficeHealthChecks();
        }
    }

    private void ConfigureBundles()
    {
        Configure<AbpBundlingOptions>(options =>
        {
            options.StyleBundles.Configure(LeptonThemeBundles.Styles.Global,
                bundle => { bundle.AddFiles("/global-styles.css"); });
        });
    }

    private void ConfigurePages(IConfiguration configuration)
    {
        Configure<RazorPagesOptions>(options =>
        {
            options.Conventions.AuthorizePage("/HostDashboard", BackOfficePermissions.Dashboard.Host);
            options.Conventions.AuthorizePage("/TenantDashboard", BackOfficePermissions.Dashboard.Tenant);
            options.Conventions.AuthorizePage("/CrawlerAccounts/Index", BackOfficePermissions.CrawlerAccounts.Default);
            options.Conventions.AuthorizePage("/CrawlerProxies/Index", BackOfficePermissions.CrawlerProxies.Default);
            options.Conventions.AuthorizePage("/CrawlerCredentials/Index", BackOfficePermissions.CrawlerCredentials.Default);
            options.Conventions.AuthorizePage("/DataSources/Index", BackOfficePermissions.DataSources.Default);
            options.Conventions.AuthorizePage("/Categories/Index", BackOfficePermissions.Categories.Default);
            options.Conventions.AuthorizePage("/Articles/Index", BackOfficePermissions.Articles.Default);
            options.Conventions.AuthorizePage("/Medias/Index", BackOfficePermissions.Medias.Default);
            options.Conventions.AuthorizePage("/Products/Index", BackOfficePermissions.Products.Default);
            options.Conventions.AuthorizePage("/ProductVariants/Index", BackOfficePermissions.ProductVariants.Default);
            options.Conventions.AuthorizePage("/ProductReviews/Index", BackOfficePermissions.ProductReviews.Default);
            options.Conventions.AuthorizePage("/ProductComments/Index", BackOfficePermissions.ProductComments.Default);
            options.Conventions.AuthorizePage("/ArticleComments/Index", BackOfficePermissions.ArticleComments.Default);
        });
    }

    private void ConfigureUrls(IConfiguration configuration)
    {
        Configure<AppUrlOptions>(options => { options.Applications["MVC"].RootUrl = configuration["App:SelfUrl"]; });
    }

    private void ConfigureAuthentication(ServiceConfigurationContext context, IConfiguration configuration)
    {
        context.Services.AddAuthentication()
            .AddJwtBearer(options =>
            {
                options.Authority = configuration["AuthServer:Authority"];
                options.RequireHttpsMetadata = Convert.ToBoolean(configuration["AuthServer:RequireHttpsMetadata"]);
                ;
                options.Audience = "BackOffice";
            });

        context.Services.ForwardIdentityAuthenticationForBearer();
    }

    private void ConfigureImpersonation(ServiceConfigurationContext context, IConfiguration configuration)
    {
        context.Services.Configure<AbpSaasHostWebOptions>(options => { options.EnableTenantImpersonation = true; });
        context.Services.Configure<AbpIdentityWebOptions>(options => { options.EnableUserImpersonation = true; });
        context.Services.Configure<AbpAccountOptions>(options =>
        {
            options.TenantAdminUserName = "admin";
            options.ImpersonationTenantPermission = SaasHostPermissions.Tenants.Impersonation;
            options.ImpersonationUserPermission = IdentityPermissions.Users.Impersonation;
        });
    }

    private void ConfigureAutoMapper()
    {
        Configure<AbpAutoMapperOptions>(options => { options.AddMaps<BackOfficeWebModule>(); });
    }

    private void ConfigureVirtualFileSystem(IWebHostEnvironment hostingEnvironment)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<BackOfficeWebModule>();

            if (hostingEnvironment.IsDevelopment())
            {
                options.FileSets.ReplaceEmbeddedByPhysical<BackOfficeDomainSharedModule>(Path.Combine(hostingEnvironment.ContentRootPath, string.Format("..{0}LC.Crawler.BackOffice.Domain.Shared", Path.DirectorySeparatorChar)));
                options.FileSets.ReplaceEmbeddedByPhysical<BackOfficeDomainModule>(Path.Combine(hostingEnvironment.ContentRootPath, string.Format("..{0}LC.Crawler.BackOffice.Domain", Path.DirectorySeparatorChar)));
                options.FileSets.ReplaceEmbeddedByPhysical<BackOfficeApplicationContractsModule>(Path.Combine(hostingEnvironment.ContentRootPath, string.Format("..{0}LC.Crawler.BackOffice.Application.Contracts", Path.DirectorySeparatorChar)));
                options.FileSets.ReplaceEmbeddedByPhysical<BackOfficeApplicationModule>(Path.Combine(hostingEnvironment.ContentRootPath, string.Format("..{0}LC.Crawler.BackOffice.Application", Path.DirectorySeparatorChar)));
                options.FileSets.ReplaceEmbeddedByPhysical<BackOfficeHttpApiModule>(Path.Combine(hostingEnvironment.ContentRootPath, string.Format("..{0}..{0}src{0}LC.Crawler.BackOffice.HttpApi", Path.DirectorySeparatorChar)));
                options.FileSets.ReplaceEmbeddedByPhysical<BackOfficeWebModule>(hostingEnvironment.ContentRootPath);
            }
        });
    }

    private void ConfigureNavigationServices()
    {
        Configure<AbpNavigationOptions>(options => { options.MenuContributors.Add(new BackOfficeMenuContributor()); });

        Configure<AbpToolbarOptions>(options => { options.Contributors.Add(new BackOfficeToolbarContributor()); });
    }

    private void ConfigureAutoApiControllers()
    {
        Configure<AbpAspNetCoreMvcOptions>(options => { options.ConventionalControllers.Create(typeof(BackOfficeApplicationModule).Assembly); });
    }

    private void ConfigureSwaggerServices(ServiceConfigurationContext context, IConfiguration configuration)
    {
        context.Services.AddAbpSwaggerGenWithOAuth(configuration["AuthServer:Authority"],
            new Dictionary<string, string> { { "BackOffice", "BackOffice API" } },
            options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "BackOffice API", Version = "v1" });
                //options.SwaggerDoc("v1-public", new OpenApiInfo { Title = "Public API", Version = "v1-public" });
                options.DocInclusionPredicate((docName, description) => true);
                options.CustomSchemaIds(type => type.FullName);
            });
    }

    private void ConfigureExternalProviders(ServiceConfigurationContext context)
    {
        context.Services.AddAuthentication()
            .AddGoogle(GoogleDefaults.AuthenticationScheme, _ => { })
            .WithDynamicOptions<GoogleOptions, GoogleHandler>(GoogleDefaults.AuthenticationScheme,
                options =>
                {
                    options.WithProperty(x => x.ClientId);
                    options.WithProperty(x => x.ClientSecret, isSecret: true);
                })
            .AddMicrosoftAccount(MicrosoftAccountDefaults.AuthenticationScheme,
                options =>
                {
                    //Personal Microsoft accounts as an example.
                    options.AuthorizationEndpoint = "https://login.microsoftonline.com/consumers/oauth2/v2.0/authorize";
                    options.TokenEndpoint = "https://login.microsoftonline.com/consumers/oauth2/v2.0/token";
                })
            .WithDynamicOptions<MicrosoftAccountOptions, MicrosoftAccountHandler>(MicrosoftAccountDefaults.AuthenticationScheme,
                options =>
                {
                    options.WithProperty(x => x.ClientId);
                    options.WithProperty(x => x.ClientSecret, isSecret: true);
                })
            .AddTwitter(TwitterDefaults.AuthenticationScheme, options => options.RetrieveUserDetails = true)
            .WithDynamicOptions<TwitterOptions, TwitterHandler>(TwitterDefaults.AuthenticationScheme,
                options =>
                {
                    options.WithProperty(x => x.ConsumerKey);
                    options.WithProperty(x => x.ConsumerSecret, isSecret: true);
                });
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var app = context.GetApplicationBuilder();
        var env = context.GetEnvironment();

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseAbpRequestLocalization();

        if (!env.IsDevelopment())
        {
            app.UseErrorPage();
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseCorrelationId();
        app.UseAbpSecurityHeaders();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthentication();
        app.UseJwtTokenMiddleware();

        if (MultiTenancyConsts.IsEnabled)
        {
            app.UseMultiTenancy();
        }

        app.UseUnitOfWork();
        app.UseIdentityServer();
        app.UseAuthorization();
        app.UseSwagger();
        app.UseAbpSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "BackOffice API");

            //options.SwaggerEndpoint("/swagger/v1-public/swagger.json", "Public API");

            var configuration = context.ServiceProvider.GetRequiredService<IConfiguration>();
            options.OAuthClientId(configuration["AuthServer:SwaggerClientId"]);
            options.OAuthClientSecret(configuration["AuthServer:SwaggerClientSecret"]);
        });
        app.UseAuditing();
        app.UseAbpSerilogEnrichers();
        app.UseConfiguredEndpoints();
        
        app.UseHangfireDashboard("/hangfire", new DashboardOptions { Authorization = new[] { new HangfireCustomBasicAuthenticationFilter { User = "admin", Pass = "123321" } } });

    }
}