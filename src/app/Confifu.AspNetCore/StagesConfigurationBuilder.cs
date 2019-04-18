using Confifu.Abstractions;

namespace Confifu.AspNetCore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using StageConfig = System.Collections.Generic.List<AspNetCoreConfigurationBuilder>;

    class StagesConfigurationBuilder
    {
        public Dictionary<string, StageConfig> Configurations { get; }
            = new Dictionary<string, StageConfig>();

        public Dictionary<string, HashSet<string>> Orders { get; }
            = new Dictionary<string, HashSet<string>>();

        public List<AspNetCoreConfigurationBuilder> Merge()
        {
            var visited = new HashSet<string>();
            var visiting = new HashSet<string>();

            var orderedConfigs = new List<AspNetCoreConfigurationBuilder>();

            IEnumerable<string> DependentStages(string stage)
            {
                return this.Orders.TryGetValue(stage, out var stages) ? stages : Enumerable.Empty<string>();
            }

            void VisitStage(string stage)
            {
                if (visiting.Contains(stage))
                    throw new InvalidOperationException($"Circular dependency detected for stage {stage}");

                if (visited.Contains(stage))
                    return;

                visited.Add(stage);
                visiting.Add(stage);

                foreach (var dependentStage in DependentStages(stage)) VisitStage(dependentStage);

                if (Configurations.TryGetValue(stage, out var configuration)) orderedConfigs.AddRange(configuration);

                visiting.Remove(stage);
            }

            void TopSort()
            {
                foreach (var stage in this.Configurations.Keys)
                    VisitStage(stage);
            }

            TopSort();

            return orderedConfigs;
        }

        public void AddConfiguration(string stage, IAppConfig appConfig, Action<AspNetCoreConfigurationBuilder> configuration)
        {
            if (string.IsNullOrEmpty(stage)) throw new ArgumentException("message", nameof(stage));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            if (!this.Configurations.TryGetValue(stage, out var stageConfig))
                this.Configurations[stage] = stageConfig = new StageConfig();

            var configurationBuilder = new AspNetCoreConfigurationBuilder(appConfig);
            configuration(configurationBuilder);
            stageConfig.Add(configurationBuilder);
        }

        public void Order(string firstStage, string nextStage)
        {
            if (string.IsNullOrEmpty(firstStage)) throw new ArgumentException("message", nameof(firstStage));

            if (string.IsNullOrEmpty(nextStage)) throw new ArgumentException("message", nameof(nextStage));

            if (!this.Orders.TryGetValue(nextStage, out var firstStages))
                this.Orders[nextStage] = firstStages = new HashSet<string>();

            firstStages.Add(firstStage);
        }
    }
}
