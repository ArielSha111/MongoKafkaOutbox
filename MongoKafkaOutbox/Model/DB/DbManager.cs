using Microsoft.Extensions.Configuration;

namespace Model.DB;
public class DbManager : IDbManager
{
    private readonly string _connectionString;

    public DbManager(IConfiguration configuration)
    {
        _connectionString = configuration.GetSection("AppSettings")["DatabaseConnectionString"];
    }

    public object GetStuffFromDB(int num)
    {
        return new object();
    }
}