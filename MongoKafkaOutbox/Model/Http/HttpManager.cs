using Microsoft.Extensions.Configuration;

namespace Model.Http;

public class HttpManager : IHttpManager
{
    private readonly string connectionString;

    public HttpManager(IConfiguration configuration)
    {
        connectionString = configuration.GetSection("AppSettings")["DatabaseConnectionString"];
    }
}
