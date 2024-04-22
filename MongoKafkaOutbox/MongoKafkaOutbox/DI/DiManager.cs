using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CachingAop.DI;

public static class DiManager
{
    public static void SetSharpCachingAopRegistration(this IServiceCollection services)
    {    
        services.TryAddSingleton<IProxyGenerator, ProxyGenerator>();
    }

    public static void AddInterceptedSingleton<TInterface, TImplementation, TInterceptor>(
    this IServiceCollection services)
        where TInterface : class
        where TImplementation : class, TInterface
        where TInterceptor : class, IInterceptor
    {
        services.AddSingleton<TImplementation>();
        services.TryAddTransient<TInterceptor>();
        services.AddSingleton(provider =>
        {
            return GetProvider<TInterface, TImplementation, TInterceptor>(provider);
        }
       );
    }

    public static void AddInterceptedTransient<TInterface, TImplementation, TInterceptor>(
    this IServiceCollection services)
        where TInterface : class
        where TImplementation : class, TInterface
        where TInterceptor : class, IInterceptor
    {
        services.AddTransient<TImplementation>();
        services.TryAddTransient<TInterceptor>();
        services.AddTransient(provider =>
        {
            return GetProvider<TInterface, TImplementation, TInterceptor>(provider);
        }
       );
    }

    public static void AddInterceptedScoped<TInterface, TImplementation, TInterceptor>(this IServiceCollection services)
        where TInterface : class
        where TImplementation : class, TInterface
        where TInterceptor : class, IInterceptor
    {
        services.AddScoped<TImplementation>();
        services.TryAddTransient<TInterceptor>();
        services.AddScoped(provider =>
        {
            return GetProvider<TInterface, TImplementation, TInterceptor>(provider);
        }
       );
    }

    public static TInterface GetProvider<TInterface, TImplementation, TInterceptor>(IServiceProvider provider)
        where TInterface : class
        where TImplementation : class, TInterface
        where TInterceptor : class, IInterceptor
    {
        var proxyGenerator = provider.GetRequiredService<IProxyGenerator>();
        var implementation = provider.GetRequiredService<TImplementation>();
        var interceptor = provider.GetRequiredService<TInterceptor>();
        return proxyGenerator.CreateInterfaceProxyWithTarget<TInterface>(implementation, interceptor);
    }
}