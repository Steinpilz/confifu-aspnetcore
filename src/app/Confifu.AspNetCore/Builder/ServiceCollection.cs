namespace Confifu.AspNetCore.Builder
{
    using System.Collections.Generic;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    class ServiceCollection : List<ServiceDescriptor>, IServiceCollection
    {
        public ServiceCollection()
        {
        }

        public ServiceCollection(IEnumerable<ServiceDescriptor> descriptors)
            : base(descriptors){}
    }
}