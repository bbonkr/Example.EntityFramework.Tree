using System;
using System.Reflection;
using FluentValidation;

namespace Example.EntityFramework.Tree.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddValidators(this IServiceCollection services, Type? type = null, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
    {
        Assembly assembly;
        if (type == null)
        {
            assembly = Assembly.GetExecutingAssembly();
        }
        else
        {
            assembly = type.Assembly;
        }

        services.AddValidatorsFromAssemblies(new List<Assembly> { assembly }, serviceLifetime);

        return services;
    }
}

