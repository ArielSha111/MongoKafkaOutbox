using Model.DB;

namespace Service;

public class ExampleService(IDbManagerWithOutBox _dbClient) : IExampleService
{
    public async Task<IEnumerable<int>> RunExample()
    {
        await _dbClient.PutStuffInDbWithOutbox();
        return new List<int>();
    }
}