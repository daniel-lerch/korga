using ChurchTools;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Threading.Tasks;

namespace Mailist.ChurchTools;

public class ChurchToolsApiFactory
{
    private readonly IHttpClientFactory clientFactory;
    private readonly IOptions<ChurchToolsOptions> options;

    public ChurchToolsApiFactory(IHttpClientFactory clientFactory, IOptions<ChurchToolsOptions> options)
    {
        this.clientFactory = clientFactory;
        this.options = options;
    }

    public ValueTask<ChurchToolsApi> Login(string username, string password)
    {
        return ChurchToolsApi.Login(clientFactory.CreateClient("ChurchTools"), options.Value.Host, username, password);
    }
}
