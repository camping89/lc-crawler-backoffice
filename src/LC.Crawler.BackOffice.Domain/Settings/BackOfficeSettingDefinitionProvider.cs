using Volo.Abp.Settings;

namespace LC.Crawler.BackOffice.Settings;

public class BackOfficeSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        //Define your own settings here. Example:
        //context.Add(new SettingDefinition(BackOfficeSettings.MySetting1));
    }
}
