using LC.Crawler.BackOffice.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;

namespace LC.Crawler.BackOffice.Permissions;

public class BackOfficePermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(BackOfficePermissions.GroupName);

        myGroup.AddPermission(BackOfficePermissions.Dashboard.Host, L("Permission:Dashboard"), MultiTenancySides.Host);
        myGroup.AddPermission(BackOfficePermissions.Dashboard.Tenant, L("Permission:Dashboard"), MultiTenancySides.Tenant);

        //Define your own permissions here. Example:
        //myGroup.AddPermission(BackOfficePermissions.MyPermission1, L("Permission:MyPermission1"));

        var crawlerAccountPermission = myGroup.AddPermission(BackOfficePermissions.CrawlerAccounts.Default, L("Permission:CrawlerAccounts"));
        crawlerAccountPermission.AddChild(BackOfficePermissions.CrawlerAccounts.Create, L("Permission:Create"));
        crawlerAccountPermission.AddChild(BackOfficePermissions.CrawlerAccounts.Edit, L("Permission:Edit"));
        crawlerAccountPermission.AddChild(BackOfficePermissions.CrawlerAccounts.Delete, L("Permission:Delete"));

        var crawlerProxyPermission = myGroup.AddPermission(BackOfficePermissions.CrawlerProxies.Default, L("Permission:CrawlerProxies"));
        crawlerProxyPermission.AddChild(BackOfficePermissions.CrawlerProxies.Create, L("Permission:Create"));
        crawlerProxyPermission.AddChild(BackOfficePermissions.CrawlerProxies.Edit, L("Permission:Edit"));
        crawlerProxyPermission.AddChild(BackOfficePermissions.CrawlerProxies.Delete, L("Permission:Delete"));

        var crawlerCredentialPermission = myGroup.AddPermission(BackOfficePermissions.CrawlerCredentials.Default, L("Permission:CrawlerCredentials"));
        crawlerCredentialPermission.AddChild(BackOfficePermissions.CrawlerCredentials.Create, L("Permission:Create"));
        crawlerCredentialPermission.AddChild(BackOfficePermissions.CrawlerCredentials.Edit, L("Permission:Edit"));
        crawlerCredentialPermission.AddChild(BackOfficePermissions.CrawlerCredentials.Delete, L("Permission:Delete"));

        var dataSourcePermission = myGroup.AddPermission(BackOfficePermissions.DataSources.Default, L("Permission:DataSources"));
        dataSourcePermission.AddChild(BackOfficePermissions.DataSources.Create, L("Permission:Create"));
        dataSourcePermission.AddChild(BackOfficePermissions.DataSources.Edit, L("Permission:Edit"));
        dataSourcePermission.AddChild(BackOfficePermissions.DataSources.Delete, L("Permission:Delete"));

        var categoryPermission = myGroup.AddPermission(BackOfficePermissions.Categories.Default, L("Permission:Categories"));
        categoryPermission.AddChild(BackOfficePermissions.Categories.Create, L("Permission:Create"));
        categoryPermission.AddChild(BackOfficePermissions.Categories.Edit, L("Permission:Edit"));
        categoryPermission.AddChild(BackOfficePermissions.Categories.Delete, L("Permission:Delete"));

        var articlePermission = myGroup.AddPermission(BackOfficePermissions.Articles.Default, L("Permission:Articles"));
        articlePermission.AddChild(BackOfficePermissions.Articles.Create, L("Permission:Create"));
        articlePermission.AddChild(BackOfficePermissions.Articles.Edit, L("Permission:Edit"));
        articlePermission.AddChild(BackOfficePermissions.Articles.Delete, L("Permission:Delete"));

        var mediaPermission = myGroup.AddPermission(BackOfficePermissions.Medias.Default, L("Permission:Medias"));
        mediaPermission.AddChild(BackOfficePermissions.Medias.Create, L("Permission:Create"));
        mediaPermission.AddChild(BackOfficePermissions.Medias.Edit, L("Permission:Edit"));
        mediaPermission.AddChild(BackOfficePermissions.Medias.Delete, L("Permission:Delete"));

        var productPermission = myGroup.AddPermission(BackOfficePermissions.Products.Default, L("Permission:Products"));
        productPermission.AddChild(BackOfficePermissions.Products.Create, L("Permission:Create"));
        productPermission.AddChild(BackOfficePermissions.Products.Edit, L("Permission:Edit"));
        productPermission.AddChild(BackOfficePermissions.Products.Delete, L("Permission:Delete"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<BackOfficeResource>(name);
    }
}