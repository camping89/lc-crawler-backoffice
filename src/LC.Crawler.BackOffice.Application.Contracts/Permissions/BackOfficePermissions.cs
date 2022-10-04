namespace LC.Crawler.BackOffice.Permissions;

public static class BackOfficePermissions
{
    public const string GroupName = "BackOffice";

    public static class Dashboard
    {
        public const string DashboardGroup = GroupName + ".Dashboard";
        public const string Host = DashboardGroup + ".Host";
        public const string Tenant = DashboardGroup + ".Tenant";
    }

    //Add your own permission names. Example:
    //public const string MyPermission1 = GroupName + ".MyPermission1";

    public class CrawlerAccounts
    {
        public const string Default = GroupName + ".CrawlerAccounts";
        public const string Edit = Default + ".Edit";
        public const string Create = Default + ".Create";
        public const string Delete = Default + ".Delete";
    }

    public class CrawlerProxies
    {
        public const string Default = GroupName + ".CrawlerProxies";
        public const string Edit = Default + ".Edit";
        public const string Create = Default + ".Create";
        public const string Delete = Default + ".Delete";
    }

    public class CrawlerCredentials
    {
        public const string Default = GroupName + ".CrawlerCredentials";
        public const string Edit = Default + ".Edit";
        public const string Create = Default + ".Create";
        public const string Delete = Default + ".Delete";
    }

    public class DataSources
    {
        public const string Default = GroupName + ".DataSources";
        public const string Edit = Default + ".Edit";
        public const string Create = Default + ".Create";
        public const string Delete = Default + ".Delete";
    }

    public class Categories
    {
        public const string Default = GroupName + ".Categories";
        public const string Edit = Default + ".Edit";
        public const string Create = Default + ".Create";
        public const string Delete = Default + ".Delete";
    }

    public class Articles
    {
        public const string Default = GroupName + ".Articles";
        public const string Edit = Default + ".Edit";
        public const string Create = Default + ".Create";
        public const string Delete = Default + ".Delete";
    }

    public class Medias
    {
        public const string Default = GroupName + ".Medias";
        public const string Edit = Default + ".Edit";
        public const string Create = Default + ".Create";
        public const string Delete = Default + ".Delete";
    }

    public class Products
    {
        public const string Default = GroupName + ".Products";
        public const string Edit = Default + ".Edit";
        public const string Create = Default + ".Create";
        public const string Delete = Default + ".Delete";
    }

    public class ProductVariants
    {
        public const string Default = GroupName + ".ProductVariants";
        public const string Edit = Default + ".Edit";
        public const string Create = Default + ".Create";
        public const string Delete = Default + ".Delete";
    }

    public class ProductAttributes
    {
        public const string Default = GroupName + ".ProductAttributes";
        public const string Edit = Default + ".Edit";
        public const string Create = Default + ".Create";
        public const string Delete = Default + ".Delete";
    }

    public class ProductReviews
    {
        public const string Default = GroupName + ".ProductReviews";
        public const string Edit = Default + ".Edit";
        public const string Create = Default + ".Create";
        public const string Delete = Default + ".Delete";
    }

    public class ProductComments
    {
        public const string Default = GroupName + ".ProductComments";
        public const string Edit = Default + ".Edit";
        public const string Create = Default + ".Create";
        public const string Delete = Default + ".Delete";
    }

    public class ArticleComments
    {
        public const string Default = GroupName + ".ArticleComments";
        public const string Edit = Default + ".Edit";
        public const string Create = Default + ".Create";
        public const string Delete = Default + ".Delete";
    }
}