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
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<BackOfficeResource>(name);
    }
}