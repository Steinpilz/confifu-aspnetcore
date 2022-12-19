using System;

namespace Confifu.AspNetCore.ConsoleHost
{
    using Abstractions;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
        }
    }

    class App : Confifu.AppSetup
    {
        public App(IConfigVariables env) : base(env)
        {
            this.Configure(this.Initial);
        }

        public void Initial()
        {
            this.AppConfig.UseAspNetCore(c => c
                .UseServiceProviderFactory(sc => null)
                .AddConfiguration(app => app
                    .ConfigureServices(sc => { })
                    .Configure(appBuilder => { })
                    .Child(childApp => childApp
                        .ConfigureServices(sc => { /* child only services */ })
                        .Configure(appBuilder => appBuilder // this appBuilder is a copy of parent IApplicationBuilder
                            .Map(new PathString("/test1"), childBuilder =>
                            {
                                // childBuilder will also be isolated
                            }))
                    )
                    .Child(childApp => childApp
                        .ConfigureServices(sc => { /* child only services */ })
                        .Configure(appBuilder => appBuilder
                            .Map(new PathString("/test2"), childBuilder =>
                            {
                                // childBuilder will also be isolated
                            }))
                    )
                )
            );
        }
    }
}