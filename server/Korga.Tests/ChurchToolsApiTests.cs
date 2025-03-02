using ChurchTools;
using System.Threading.Tasks;
using Xunit;

namespace Korga.Tests;

public class ChurchToolsApiTests
{
    private const string churchToolsHost = "demo.church.tools";
    private const string churchToolsUsername = "churchtools";
    private const string churchToolsPassword = "churchtools";

    [Fact]
    public async Task TestLogin()
    {
        using ChurchToolsApi client = await ChurchToolsApi.Login(churchToolsHost, churchToolsUsername, churchToolsPassword);
        
        // This call fails if client is not authenticated
        await client.GetPerson();
    }

    [Fact]
    public async Task TestCreateWithToken()
    {
        // This test depends on login because login tokens might change without notice
        using ChurchToolsApi bootstrap = await ChurchToolsApi.Login(churchToolsHost, churchToolsUsername, churchToolsPassword);
        Assert.NotNull(bootstrap.User);

        string loginToken = await bootstrap.GetPersonLoginToken(bootstrap.User.PersonId);

        using ChurchToolsApi client = ChurchToolsApi.CreateWithToken(churchToolsHost, loginToken);

        // This call fails if client is not authenticated
        await client.GetPerson();
    }

    [Fact]
    public async Task TestGetGlobalPermissions()
    {
        using ChurchToolsApi client = await ChurchToolsApi.Login(churchToolsHost, churchToolsUsername, churchToolsPassword);

        await client.GetGlobalPermissions();
    }
}
