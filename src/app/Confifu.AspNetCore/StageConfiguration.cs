namespace Confifu.AspNetCore
{
    using System;

    class StageConfiguration
    {
        public StageConfiguration(string stage, Action<AspNetCoreConfigurationBuilder> configuration)
        {
            this.Stage = stage;
            this.Configuration = configuration;
        }

        public string Stage { get; }
        public Action<AspNetCoreConfigurationBuilder> Configuration { get; }
    }
}