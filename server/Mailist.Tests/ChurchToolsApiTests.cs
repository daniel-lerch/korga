using ChurchTools;
using ChurchTools.Model;
using Mailist.EmailRelay;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Mailist.Tests;

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
        await client.GetPerson(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestCreateWithToken()
    {
        // This test depends on login because login tokens might change without notice
        using ChurchToolsApi bootstrap = await ChurchToolsApi.Login(churchToolsHost, churchToolsUsername, churchToolsPassword);
        Assert.NotNull(bootstrap.User);

        string loginToken = await bootstrap.GetPersonLoginToken(bootstrap.User.PersonId, TestContext.Current.CancellationToken);

        using ChurchToolsApi client = ChurchToolsApi.CreateWithToken(churchToolsHost, loginToken);

        // This call fails if client is not authenticated
        await client.GetPerson(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestGetGlobalPermissions()
    {
        using ChurchToolsApi client = await ChurchToolsApi.Login(churchToolsHost, churchToolsUsername, churchToolsPassword);

        await client.GetGlobalPermissions(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestChurchQueryEmail()
    {
        using ChurchToolsApi client = await ChurchToolsApi.Login(churchToolsHost, churchToolsUsername, churchToolsPassword);

        ChurchQueryRequest<IdNameEmail> query = new(JsonElement.Parse("{ \"==\": [{ \"var\": \"person.email\" }, \"support@example.com\"] }"));

        var results = await client.ChurchQuery(query, TestContext.Current.CancellationToken);

        Assert.NotNull(results);
        var arminAdendorf = results.Single();
        Assert.Equal(1, arminAdendorf.Id);
        Assert.Equal("Armin", arminAdendorf.FirstName);
        Assert.Equal("Adendorf", arminAdendorf.LastName);
        Assert.Equal("support@example.com", arminAdendorf.Email);
    }

    [Fact]
    public async Task TestChurchQueryGroup()
    {
        using ChurchToolsApi client = await ChurchToolsApi.Login(churchToolsHost, churchToolsUsername, churchToolsPassword);

        ChurchQueryRequest<IdNameEmail> query = new(JsonElement.Parse("""
            {
              "and": [
                { "==": [{ "var": "person.isArchived" }, 0] },
                { "isnull": [{ "var": "person.dateOfDeath" }] },
                { "==": [{ "var": "groupmember.groupMemberStatus" }, "active"] },
                { "oneof": [{ "var": "ctgroup.id" }, ["7"]] }
              ]
            }
            """));

        var results = await client.ChurchQuery(query, TestContext.Current.CancellationToken);

        Assert.NotNull(results);
        Assert.True(results.Count >= 9);
        var arminAdendorf = results.Single(p => p.Id == 1);
        Assert.Equal("Armin", arminAdendorf.FirstName);
        Assert.Equal("Adendorf", arminAdendorf.LastName);
        Assert.Equal("support@example.com", arminAdendorf.Email);
    }

}
