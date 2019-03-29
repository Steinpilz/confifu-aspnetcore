namespace Confifu.AspNetCore
{
    using System;

    using Abstractions;
    using Builder;
    using Microsoft.Extensions.DependencyInjection;

    public static class AppConfigExtensions
    {
        public class Config
        {
            readonly IAppConfig appConfig;

            Func<IServiceCollection, IServiceProvider> serviceProviderFactory;
            readonly StagesConfigurationBuilder stagesConfiguration;

            internal Config(IAppConfig appConfig)
            {
                this.appConfig = appConfig;
                this.stagesConfiguration = new StagesConfigurationBuilder();
            }

            internal void InitDefaults()
            {
                this.appConfig.SetAspNetCoreConfigurationFactory(() =>
                {
                    var builders = this.stagesConfiguration.Merge();
                    var factory = new AspNetCoreConfigurationFactory(builders, this.serviceProviderFactory);
                    return factory.Create();
                });

                this.appConfig.AddAppRunnerAfter(() =>
                {
                    if (this.serviceProviderFactory == null)
                    {
                        throw new InvalidOperationException("ServiceProviderFactory is not configured");
                    }
                });
            }

            public Config AddConfiguration(Action<AspNetCoreConfigurationBuilder>  configuration) =>
                this.AddConfiguration("default", configuration);

            public Config AddConfiguration(string stage, Action<AspNetCoreConfigurationBuilder>  configuration)
            {
                this.stagesConfiguration.AddConfiguration(stage, appConfig, configuration);
                return this;
            }

            public Config OrderStages(params string[] stages)
            {
                for(var i = 0; i < stages.Length-1; i++)
                {
                    this.OrderStages(stages[i], stages[i + 1]);
                }
                return this;
            }

            public Config OrderStages(string firstStage, string nextStage)
            {
                this.stagesConfiguration.Order(firstStage, nextStage);
                return this;
            }

            public Config UseServiceProviderFactory(Func<IServiceCollection, IServiceProvider> factory)
            {
                this.serviceProviderFactory = factory ?? throw new ArgumentNullException(nameof(factory));
                return this;
            }
        }

        public static IAppConfig UseAspNetCore(this IAppConfig appConfig, Action<Config> configurator = null)
        {
            var config = appConfig.EnsureConfig(
                "AspNetCore",
                () => new Config(appConfig),
                c => { c.InitDefaults(); });

            configurator?.Invoke(config);

            return appConfig;
        }

        internal static IAppConfig SetAspNetCoreConfigurationFactory(
            this IAppConfig appConfig,
            Func<AspNetCoreConfiguration> configurationFactory
        )
        {
            appConfig["AspNetCore:ConfigurationFactory"] = configurationFactory;
            return appConfig;
        }

        public static AspNetCoreConfiguration GetAspNetCoreConfiguration(this IAppConfig appConfig)
        {
            var configurationFactory = appConfig["AspNetCore:ConfigurationFactory"] as Func<AspNetCoreConfiguration>;
            return configurationFactory?.Invoke() ?? AspNetCoreConfiguration.Empty;
        }
    }
}
