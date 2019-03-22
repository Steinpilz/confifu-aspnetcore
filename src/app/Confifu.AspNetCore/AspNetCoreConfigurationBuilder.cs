namespace Confifu.AspNetCore
{
    using System;
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;

    public class AspNetCoreConfigurationBuilder
    {
        internal List<Action<IApplicationBuilder>> ApplicationBuilderConfigurators { get; } =
            new List<Action<IApplicationBuilder>>();

        internal List<AspNetCoreConfigurationBuilder> ChildBuilders { get; } =
            new List<AspNetCoreConfigurationBuilder>();

        internal List<Action<IServiceCollection>> ServiceConfigurators { get; } =
            new List<Action<IServiceCollection>>();

        internal Func<HttpContext, bool> MapWhen { get; private set; }

        internal PathString MapPath { get; private set; }

        public AspNetCoreConfigurationBuilder ConfigureServices(Action<IServiceCollection> configurator)
        {
            if (configurator == null) throw new ArgumentNullException(nameof(configurator));
            this.ServiceConfigurators.Add(configurator);
            return this;
        }

        public AspNetCoreConfigurationBuilder Configure(Action<IApplicationBuilder> configurator)
        {
            if (configurator == null) throw new ArgumentNullException(nameof(configurator));
            this.ApplicationBuilderConfigurators.Add(configurator);
            return this;
        }

        public AspNetCoreConfigurationBuilder Child(Action<AspNetCoreConfigurationBuilder> childConfigurator)
        {
            if (childConfigurator == null) throw new ArgumentNullException(nameof(childConfigurator));
            
            var child = new AspNetCoreConfigurationBuilder();
            childConfigurator(child);
            this.ChildBuilders.Add(child);
            
            return this;
        }

        public AspNetCoreConfigurationBuilder ChildMapWhen(
            Func<HttpContext, bool> predicate,
            Action<AspNetCoreConfigurationBuilder> childConfigurator)
        {
            if (childConfigurator == null) throw new ArgumentNullException(nameof(childConfigurator));
            
            var child = new AspNetCoreConfigurationBuilder();
            child.MapWhen = predicate;
            childConfigurator(child);
            this.ChildBuilders.Add(child);
            
            return this;
        }

        public AspNetCoreConfigurationBuilder ChildMapPath(
            PathString path,
            Action<AspNetCoreConfigurationBuilder> childConfigurator)
        {
            if (childConfigurator == null) throw new ArgumentNullException(nameof(childConfigurator));
            
            var child = new AspNetCoreConfigurationBuilder();
            child.MapPath = path;
            childConfigurator(child);
            this.ChildBuilders.Add(child);
            
            return this;
        }
    }
}