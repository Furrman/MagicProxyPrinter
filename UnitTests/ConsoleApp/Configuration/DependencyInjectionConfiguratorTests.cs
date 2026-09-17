using System.Text.RegularExpressions;
using ConsoleApp.Configuration;

namespace UnitTests.ConsoleApp.Configuration;

public class DependencyInjectionConfiguratorTests
{
    [Fact]
    public void GetAppVersion_ReturnsThreePartVersionString()
    {
        var version = DependencyInjectionConfigurator.GetAppVersion();

        Assert.Matches(new Regex(@"^\d+\.\d+\.\d+$"), version);
    }
}
