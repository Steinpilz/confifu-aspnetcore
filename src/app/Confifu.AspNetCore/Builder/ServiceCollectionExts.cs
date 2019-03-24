namespace Confifu.AspNetCore.Builder
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Microsoft.Extensions.DependencyInjection;

    static class ServiceCollectionExts
    {
        static readonly MethodInfo GetServiceMethod = typeof(IServiceProvider).GetMethod("GetService");

        public static IServiceCollection AppProxiedServices(
            this IServiceCollection services,
            IServiceProvider proxyServiceProvider,
            IServiceCollection servicesToProxy)
        {
            foreach (var descriptor in servicesToProxy)
            {
                services.Add(CreateProxyServiceDescriptor(proxyServiceProvider, descriptor));
            }

            return services;
        }

        static ServiceDescriptor CreateProxyServiceDescriptor(IServiceProvider sp, ServiceDescriptor original)
        {
            if (original.ServiceType.IsGenericTypeDefinition)
            {
                // it's safe for transient lifetime only, but there is no alternatives by now
                return original;
            }
            
            var implementationType = TryGetImplementationType(original);

            var factory = 
                implementationType == null || implementationType == typeof(object)
                ? _ => sp.GetService(original.ServiceType)
                : (Func<IServiceProvider, object>)GetTypedFactory(sp, original.ServiceType, implementationType);
            
            return new ServiceDescriptor(
                original.ServiceType,
                factory,
                ServiceLifetime.Transient);
        }

        static Delegate GetTypedFactory(IServiceProvider sp, Type serviceType, Type implementationType)
        {
            var spArg = Expression.Parameter(typeof(IServiceProvider), "sp");

            var body = Expression.TypeAs(
                Expression.Call(Expression.Constant(sp), GetServiceMethod, Expression.Constant(serviceType)),
                implementationType);

            return Expression.Lambda(body, new[] {spArg}).Compile();
        }

        static Type TryGetImplementationType(ServiceDescriptor sd)
        {
            if (sd.ImplementationType != null)
                return sd.ImplementationType;
            if (sd.ImplementationInstance != null)
                return sd.ImplementationInstance.GetType();
            if (sd.ImplementationFactory != null)
                return sd.ImplementationFactory.GetType().GenericTypeArguments[1];
            return null;
        }
    }
}