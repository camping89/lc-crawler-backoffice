using Volo.Abp.Identity;

namespace LC.Crawler.BackOffice;

public static class BackOfficeConsts
{
    public const string DbTablePrefix = "App";
    public const string DbSchema = null;
    public const string AdminEmailDefaultValue = IdentityDataSeedContributor.AdminEmailDefaultValue;
    public const string AdminPasswordDefaultValue = IdentityDataSeedContributor.AdminPasswordDefaultValue;
}
