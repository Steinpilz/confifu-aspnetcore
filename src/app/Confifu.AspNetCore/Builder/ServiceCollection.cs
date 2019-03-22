namespace Confifu.AspNetCore.Builder
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.DependencyInjection;

    class ServiceCollection : List<ServiceDescriptor>, IServiceCollection
    {
        public ServiceCollection(params IServiceCollection[] collections)
        {
            this.AddRange(collections.SelectMany(x => x));
        }
    }
}