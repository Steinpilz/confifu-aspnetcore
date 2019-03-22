namespace Confifu.AspNetCore.Autofac
{
    using System;

    using Abstractions;
    using global::Autofac;
    using global::Autofac.Extensions.DependencyInjection;

    public static class AppConfigExtensions
    {
        public class Config
        {
            readonly IAppConfig appConfig;
            
            internal Config(IAppConfig appConfig)
            {
                this.appConfig = appConfig;
            }

            internal void InitDefaults()
            {
                this.appConfig
                    .UseAspNetCore(c => c
                        .UseServiceProviderFactory(sc =>
                        {
                            var builder = new ContainerBuilder();

                            builder.Populate(sc);

                            var container = builder.Build();
                            return container.Resolve<IServiceProvider>();
                        }));
            }
            
        }

        public static IAppConfig UseAspNetCoreAutofac(this IAppConfig appConfig, Action<Config> configurator = null)
        {
            var config = appConfig.EnsureConfig(
                "AspNetCore:Autofac",
                () => new Config(appConfig),
                c => { c.InitDefaults(); });

            configurator?.Invoke(config);

            return appConfig;
        }
    }
}
