using Model.DB;

namespace Service;

public class ExampleService(IDbManager _dbClient) : IExampleService
{
    public async Task<IEnumerable<int>> RunExample()
    {
        _dbClient.GetStuffFromDB(1);
        _dbClient.GetStuffFromDB(1);
        _dbClient.GetStuffFromDB(2);
        return new List<int>();
    }
}