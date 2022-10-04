using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using LC.Crawler.BackOffice.Localization;
using LC.Crawler.BackOffice.Permissions;
using Volo.Abp.AuditLogging.Web.Navigation;
using Volo.Abp.Identity.Web.Navigation;
using Volo.Abp.IdentityServer.Web.Navigation;
using Volo.Abp.LanguageManagement.Navigation;
using Volo.Abp.SettingManagement.Web.Navigation;
using Volo.Abp.TextTemplateManagement.Web.Navigation;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.UI.Navigation;
using Volo.Saas.Host.Navigation;

namespace LC.Crawler.BackOffice.Web.Menus;

public class BackOfficeMenuContributor : IMenuContributor
{
    public async Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        if (context.Menu.Name == StandardMenus.Main)
        {
            await ConfigureMainMenuAsync(context);
        }
    }

    private static Task ConfigureMainMenuAsync(MenuConfigurationContext context)
    {
        var l = context.GetLocalizer<BackOfficeResource>();

        //Home
        context.Menu.AddItem(
            new ApplicationMenuItem(
                BackOfficeMenus.Home,
                l["Menu:Home"],
                "~/",
                icon: "fa fa-home",
                order: 1
            )
        );

        //HostDashboard
        context.Menu.AddItem(
            new ApplicationMenuItem(
                BackOfficeMenus.HostDashboard,
                l["Menu:Dashboard"],
                "~/HostDashboard",
                icon: "fa fa-line-chart",
                order: 2
            ).RequirePermissions(BackOfficePermissions.Dashboard.Host)
        );

        //TenantDashboard
        context.Menu.AddItem(
            new ApplicationMenuItem(
                BackOfficeMenus.TenantDashboard,
                l["Menu:Dashboard"],
                "~/Dashboard",
                icon: "fa fa-line-chart",
                order: 2
            ).RequirePermissions(BackOfficePermissions.Dashboard.Tenant)
        );

        context.Menu.SetSubItemOrder(SaasHostMenuNames.GroupName, 3);

        //Administration
        var administration = context.Menu.GetAdministration();
        administration.Order = 5;

        //Administration->Identity
        administration.SetSubItemOrder(IdentityMenuNames.GroupName, 1);

        //Administration->Identity Server
        administration.SetSubItemOrder(AbpIdentityServerMenuNames.GroupName, 2);

        //Administration->Language Management
        administration.SetSubItemOrder(LanguageManagementMenuNames.GroupName, 3);

        //Administration->Text Template Management
        administration.SetSubItemOrder(TextTemplateManagementMainMenuNames.GroupName, 4);

        //Administration->Audit Logs
        administration.SetSubItemOrder(AbpAuditLoggingMainMenuNames.GroupName, 5);

        //Administration->Settings
        administration.SetSubItemOrder(SettingManagementMenuNames.GroupName, 6);

        context.Menu.AddItem(
            new ApplicationMenuItem(
                BackOfficeMenus.CrawlerAccounts,
                l["Menu:CrawlerAccounts"],
                url: "/CrawlerAccounts",
                icon: "fa fa-file-alt",
                requiredPermissionName: BackOfficePermissions.CrawlerAccounts.Default)
        );

        context.Menu.AddItem(
            new ApplicationMenuItem(
                BackOfficeMenus.CrawlerProxies,
                l["Menu:CrawlerProxies"],
                url: "/CrawlerProxies",
                icon: "fa fa-file-alt",
                requiredPermissionName: BackOfficePermissions.CrawlerProxies.Default)
        );

        context.Menu.AddItem(
            new ApplicationMenuItem(
                BackOfficeMenus.CrawlerCredentials,
                l["Menu:CrawlerCredentials"],
                url: "/CrawlerCredentials",
                icon: "fa fa-file-alt",
                requiredPermissionName: BackOfficePermissions.CrawlerCredentials.Default)
        );

        context.Menu.AddItem(
            new ApplicationMenuItem(
                BackOfficeMenus.DataSources,
                l["Menu:DataSources"],
                url: "/DataSources",
                icon: "fa fa-file-alt",
                requiredPermissionName: BackOfficePermissions.DataSources.Default)
        );

        context.Menu.AddItem(
            new ApplicationMenuItem(
                BackOfficeMenus.Categories,
                l["Menu:Categories"],
                url: "/Categories",
                icon: "fa fa-file-alt",
                requiredPermissionName: BackOfficePermissions.Categories.Default)
        );

        context.Menu.AddItem(
            new ApplicationMenuItem(
                BackOfficeMenus.Articles,
                l["Menu:Articles"],
                url: "/Articles",
                icon: "fa fa-file-alt",
                requiredPermissionName: BackOfficePermissions.Articles.Default)
        );

        context.Menu.AddItem(
            new ApplicationMenuItem(
                BackOfficeMenus.Medias,
                l["Menu:Medias"],
                url: "/Medias",
                icon: "fa fa-file-alt",
                requiredPermissionName: BackOfficePermissions.Medias.Default)
        );

        context.Menu.AddItem(
            new ApplicationMenuItem(
                BackOfficeMenus.Products,
                l["Menu:Products"],
                url: "/Products",
                icon: "fa fa-file-alt",
                requiredPermissionName: BackOfficePermissions.Products.Default)
        );

        context.Menu.AddItem(
            new ApplicationMenuItem(
                BackOfficeMenus.ProductVariants,
                l["Menu:ProductVariants"],
                url: "/ProductVariants",
                icon: "fa fa-file-alt",
                requiredPermissionName: BackOfficePermissions.ProductVariants.Default)
        );

        context.Menu.AddItem(
            new ApplicationMenuItem(
                BackOfficeMenus.ProductReviews,
                l["Menu:ProductReviews"],
                url: "/ProductReviews",
                icon: "fa fa-file-alt",
                requiredPermissionName: BackOfficePermissions.ProductReviews.Default)
        );

        context.Menu.AddItem(
            new ApplicationMenuItem(
                BackOfficeMenus.ProductComments,
                l["Menu:ProductComments"],
                url: "/ProductComments",
                icon: "fa fa-file-alt",
                requiredPermissionName: BackOfficePermissions.ProductComments.Default)
        );

        context.Menu.AddItem(
            new ApplicationMenuItem(
                BackOfficeMenus.ArticleComments,
                l["Menu:ArticleComments"],
                url: "/ArticleComments",
                icon: "fa fa-file-alt",
                requiredPermissionName: BackOfficePermissions.ArticleComments.Default)
        );
        return Task.CompletedTask;
    }
}