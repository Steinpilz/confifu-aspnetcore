namespace Confifu.AspNetCore.Builder
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Features;
    using Microsoft.Extensions.DependencyInjection;

    class AspNetCoreConfigurationFactory
    {
        readonly AspNetCoreConfigurationBuilder builder;
        readonly Func<IServiceCollection, IServiceProvider> serviceProviderFactory;

        public AspNetCoreConfigurationFactory(
            AspNetCoreConfigurationBuilder builder,
            Func<IServiceCollection, IServiceProvider> serviceProviderFactory
        )
        {
            this.builder = builder;
            this.serviceProviderFactory = serviceProviderFactory;
        }

        public AspNetCoreConfiguration Create()
        {
            return new AspNetCoreConfiguration(
                ConcatActions(this.builder.ServiceConfigurators),
                this.BuildUpApplicationBuilderForRoot
            );
        }

        void BuildUpApplicationBuilderForRoot(
            IServiceCollection services,
            IApplicationBuilder applicationBuilder
        )
        {
            this.BuildUpApplicationBuilder(services, applicationBuilder, this.builder);
        }

        void BuildUpApplicationBuilder(
            IServiceCollection services,
            IApplicationBuilder app,
            AspNetCoreConfigurationBuilder configBuilder
        )
        {
            configBuilder.ApplicationBuilderConfigurators.ForEach(f => f(app));

            foreach (var childBuilder in configBuilder.ChildBuilders)
            {
                var childApp = app.New();
                var childServices = new ServiceCollection(services);
                childBuilder.ServiceConfigurators.ForEach(f => f(childServices));

                var childServiceProvider = this.serviceProviderFactory(childServices);

                childApp.ApplicationServices = childServiceProvider;

                this.BuildUpApplicationBuilder(childServices, childApp, childBuilder);
                SetupChildMiddleware(app, childApp, childBuilder);
            }
        }

        static void SetupChildMiddleware(
            IApplicationBuilder app,
            IApplicationBuilder childApp,
            AspNetCoreConfigurationBuilder childBuilder
        )
        {
            var branch = childApp.Build();
            var scopeFactory = childApp.ApplicationServices.GetService<IServiceScopeFactory>();

            if (childBuilder.MapWhen != null)
            {
                app.Use(async (ctx, next) =>
                {
                    if (childBuilder.MapWhen(ctx))
                    {
                        using (ServiceProvidersFeatureScope(ctx, scopeFactory))
                        {
                            await branch(ctx);
                        }
                    }
                    else
                    {
                        await next();
                    }
                });
            }
            else if (childBuilder.MapPath != null)
            {
                app.Use(async (ctx, next) =>
                {
                    if (ctx.Request.Path.StartsWithSegments(childBuilder.MapPath, out var matchedPath, out var remainingPath))
                    {
                        // Update the path
                        var path = ctx.Request.Path;
                        var pathBase = ctx.Request.PathBase;
                        ctx.Request.PathBase = pathBase.Add(matchedPath);
                        ctx.Request.Path = remainingPath;

                        try
                        {
                            using (ServiceProvidersFeatureScope(ctx, scopeFactory))
                            {
                                await branch(ctx);
                            }
                        }
                        finally
                        {
                            ctx.Request.PathBase = pathBase;
                            ctx.Request.Path = path;
                        }
                    }
                    else
                    {
                        await next();
                    }
                });
            }
            else
            {
                app.Use(async (ctx, next) =>
                {
                    using (ServiceProvidersFeatureScope(ctx, scopeFactory))
                    {
                        await branch(ctx);
                    }

                    await next();
                });
            }
        }

        static IDisposable FeatureScope<T>(HttpContext ctx, T feature) => new FeatureScope<T>(ctx.Features, feature);

        static IDisposable ServiceProvidersFeatureScope(HttpContext ctx, IServiceScopeFactory serviceScopeFactory) =>
            FeatureScope<IServiceProvidersFeature>(ctx, new RequestServicesFeature(ctx, serviceScopeFactory));

        static Action<T> ConcatActions<T>(IEnumerable<Action<T>> actions) =>
            actions.Aggregate((Action<T>) (_ => { }), (a, x) => a + x);
    }
}
