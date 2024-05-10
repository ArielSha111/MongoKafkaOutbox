namespace Model.DB;

public interface IDbManagerWithOutBox
{
    public Task PutStuffInDbWithOutbox();
}