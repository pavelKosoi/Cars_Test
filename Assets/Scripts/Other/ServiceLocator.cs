using System;
using System.Collections.Generic;

public static class ServiceLocator
{
    static readonly Dictionary<Type, object> services = new Dictionary<Type, object>();

    public static void Register<T>(T service)
    {
        var type = typeof(T);
        if (!services.ContainsKey(type))
        {
            services[type] = service;
        }
    }

    public static T Get<T>()
    {
        var type = typeof(T);
        if (services.TryGetValue(type, out var service))
        {
            return (T)service;
        }
        throw new Exception($"Service {type} is not registered in ServiceLocator!");
    }

    public static void Clear()
    {
        services.Clear();
    }
}