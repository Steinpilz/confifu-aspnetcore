namespace Confifu.AspNetCore
{
    using System;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;

    public class AspNetCoreConfiguration
    {
        internal static readonly AspNetCoreConfiguration Empty = new AspNetCoreConfiguration(_ => { }, (_,__) => { });

        public Action<IServiceCollection> ConfigureServices { get; }

        public Action<IServiceCollection, IApplicationBuilder> ConfigureApplicationBuilder { get; }

        public AspNetCoreConfiguration(
            Action<IServiceCollection> configureServices,
            Action<IServiceCollection, IApplicationBuilder> configureApplicationBuilder)
        {
            this.ConfigureServices = configureServices ?? throw new ArgumentNullException(nameof(configureServices));
            this.ConfigureApplicationBuilder = configureApplicationBuilder
                                               ?? throw new ArgumentNullException(nameof(configureApplicationBuilder));
        }
    }
}