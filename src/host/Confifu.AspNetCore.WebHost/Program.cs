using System;
using Confifu.Abstractions;
using Confifu.Abstractions.DependencyInjection;
using Confifu.AspNetCore.Autofac;

using global::Autofac.Extensions.DependencyInjection;
using Autofac;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json.Serialization;

namespace Confifu.AspNetCore.WebHost
{

    public class Program
    {
        static Func<int, object> foo;

        public static void Main(string[] args)
        {
            Func<int, string> fs = i => "123";
            foo = fs;

            var returnType = foo.GetType().GetGenericArguments()[1];

            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            Microsoft.AspNetCore.WebHost.CreateDefaultBuilder(args)
                .UseStartup<AppSetupStartup>();
    }

    class AppSetupStartup : StartupBase
    {
        readonly App confifuApp;
        readonly AspNetCoreConfiguration aspNetCoreConfig;

        public AppSetupStartup()
        {
            this.confifuApp = new App(new EmptyConfigVariables());
            this.confifuApp.Setup().Run();

            this.aspNetCoreConfig = this.confifuApp.AppConfig.GetAspNetCoreConfiguration();
        }

        public override void Configure(IApplicationBuilder app)
        {
            var services = app.ApplicationServices.GetService<IServiceCollection>();

            this.aspNetCoreConfig.ConfigureApplicationBuilder(services, app);
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            var confifuServices = this.confifuApp.AppConfig.GetServiceCollection();
            var confifuServiceProvider = this.confifuApp.AppConfig.GetServiceProvider();

            services.AppProxiedServices(
                confifuServiceProvider, confifuServices);

            this.aspNetCoreConfig.ConfigureServices(services);

            services.Replace(ServiceDescriptor.Transient<IServiceCollection, IServiceCollection>(
                sp => services));
        }

        public override IServiceProvider CreateServiceProvider(IServiceCollection services)
        {
            var builder = new ContainerBuilder();

            builder.Populate(services);

            var container = builder.Build();
            return container.Resolve<IServiceProvider>();
        }
    }

    class App : Confifu.AppSetup
    {
        public App(IConfigVariables env) : base(env)
        {
            this.Configure(this.Initial);
        }

        void Initial()
        {
            this.AppConfig
                .RegisterServices(sc =>
                {
                    sc.AddTransient<ServiceA>(sp => new ServiceA("RegisterServices"));
                    sc.AddTransient<ServiceB>(sp => new ServiceB("RegisterServices"));
                })
                .AddAppRunnerAfter(() =>
                {
                    var builder = new ContainerBuilder();

                    builder.Populate(this.AppConfig.GetServiceCollection());

                    var container = builder.Build();
                    this.AppConfig.SetServiceProvider(container.Resolve<IServiceProvider>());
                })
                .UseAspNetCoreAutofac()
                .UseAspNetCore(aspNetCore => aspNetCore
                    .AddConfiguration(c => c
                        .ChildMapPath(PathString.FromUriComponent("/test1"), test1 => test1
                            .ConfigureServices(services =>
                            {
                                services.Replace(ServiceDescriptor.Transient<ServiceB, ServiceB>(
                                    sp => new ServiceB("test1")));

                                services.AddMvc()
                                    .ConfigureApplicationPartManager(pm =>
                                    {

                                    })
                                    .AddNewtonsoftJson(json =>
                                    {
                                        json.SerializerSettings.ContractResolver =
                                            new CamelCasePropertyNamesContractResolver();
                                    });
                            })
                            .Configure(app =>
                            {
                                app.UseHttpsRedirection();
                            })
                            .Configure(app =>
                            {
                                app.UseRouting();
                                app.UseAuthorization();
                                app.UseEndpoints(endpoints =>
                                {
                                    endpoints.MapRazorPages();
                                    endpoints.MapControllers();
                                });
                            })
                        )
                        .ChildMapPath(PathString.FromUriComponent("/test2"), test1 => test1
                            .ConfigureServices(services =>
                            {
                                services.Replace(ServiceDescriptor.Transient<ServiceB, ServiceB>(
                                    sp => new ServiceB("test2")));

                                services.AddMvc()
                                    .AddNewtonsoftJson(json =>
                                    {
                                        json.SerializerSettings.ContractResolver =
                                            new DefaultContractResolver();
                                    });
                            })
                            .Configure(app =>
                            {
                                app.UseHttpsRedirection();
                            })
                            .Configure(app =>
                            {
                                app.UseRouting();
                                app.UseAuthorization();
                                app.UseEndpoints(endpoints =>
                                {
                                    endpoints.MapRazorPages();
                                    endpoints.MapControllers();
                                });
                            })
                        )
                        .ConfigureServices(services =>
                        {
                            services.AddMvc()
                                .AddNewtonsoftJson(json =>
                                {
                                    json.SerializerSettings.ContractResolver =
                                        new DefaultContractResolver();
                                });
                        })
                        .Configure(app =>
                        {
                            app.UseHttpsRedirection();
                        })
                        .Configure(app =>
                        {
                            app.UseRouting();
                            app.UseAuthorization();
                            app.UseEndpoints(endpoints =>
                            {
                                endpoints.MapGet("/", () => "Confifu Asp.Net Core application");
                            });
                        })
                    )
                );
        }
    }
}
