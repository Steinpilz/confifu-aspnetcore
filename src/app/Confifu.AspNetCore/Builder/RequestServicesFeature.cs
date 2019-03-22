namespace Confifu.AspNetCore.Builder
{
    using System;
    using System.Diagnostics;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Features;
    using Microsoft.Extensions.DependencyInjection;

    class RequestServicesFeature : IServiceProvidersFeature, IDisposable
    {
        readonly IServiceScopeFactory scopeFactory;
        readonly HttpContext context;
        IServiceProvider requestServices;
        bool requestServicesSet;
        IServiceScope scope;

        public RequestServicesFeature(HttpContext context, IServiceScopeFactory scopeFactory)
        {
            Debug.Assert(scopeFactory != null);
            this.context = context;
            this.scopeFactory = scopeFactory;
        }

        public void Dispose()
        {
            this.scope?.Dispose();
            this.scope = null;
            this.requestServices = null;
        }

        public IServiceProvider RequestServices
        {
            get
            {
                if (!this.requestServicesSet)
                {
                    this.context.Response.RegisterForDispose(this);
                    this.scope = this.scopeFactory.CreateScope();
                    this.requestServices = this.scope.ServiceProvider;
                    this.requestServicesSet = true;
                }

                return this.requestServices;
            }

            set
            {
                this.requestServices = value;
                this.requestServicesSet = true;
            }
        }
    }
}