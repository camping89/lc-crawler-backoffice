using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace LC.Crawler.BackOffice.Pages;

[Collection(BackOfficeTestConsts.CollectionDefinitionName)]
public class Index_Tests : BackOfficeWebTestBase
{
    [Fact]
    public async Task Welcome_Page()
    {
        var response = await GetResponseAsStringAsync("/");
        response.ShouldNotBeNull();
    }
}
