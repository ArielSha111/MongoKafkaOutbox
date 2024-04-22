using Castle.DynamicProxy;
using MongoKafkaOutbox.Mongo;
using System.Reflection;


namespace CachingAop.Interceptors;

public class OutboxInterceptor(IMongoOutbox mongoOutbox) : IInterceptor
{
    private readonly List<IInvocationProceedInfo> captureProceedInfos = new List<IInvocationProceedInfo>();

   
    public void Intercept(IInvocation invocation)
    {
        if (IsSaveChangesMethod(invocation.Method))
        {
       
            using var session = await _database.Client.StartSessionAsync();
            session.StartTransaction();
            try
            {
                foreach (IInvocationProceedInfo captureProceedInfo in captureProceedInfos)
                {
                    captureProceedInfo.Invoke();
                }
                await session.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                await session.AbortTransactionAsync();
                throw ex;
            }

           
        }

        if (IsVoidOrTaskReturnType(invocation.Method))
           invocation.Proceed();

        captureProceedInfos.Add(invocation.CaptureProceedInfo());
    }

    private bool IsSaveChangesMethod(MethodInfo method)
    {
        return method.Name == nameof(IMongoOutbox.SaveChanges);
    }

    private bool IsVoidOrTaskReturnType(MethodInfo method)
    {
        var returnType = method.ReturnType;
        return returnType == typeof(void) || (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>));
    }
}
