namespace Confifu.AspNetCore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    class StagesConfigurationBuilder
    {
        public Dictionary<string, List<StageConfiguration>> Configurations { get; }
            = new Dictionary<string, List<StageConfiguration>>();

        public Dictionary<string, HashSet<string>> Orders { get; }
            = new Dictionary<string, HashSet<string>>();

        public Action<AspNetCoreConfigurationBuilder> Build()
        {
            var visited = new HashSet<string>();
            var visiting = new HashSet<string>();

            var orderedStages = new List<StageConfiguration>();

            IEnumerable<string> DependentStages(string stage)
            {
                return this.Orders.TryGetValue(stage, out var stages) ? stages : Enumerable.Empty<string>();
            }

            IEnumerable<StageConfiguration> Configuration(string stage)
            {
                return this.Configurations.TryGetValue(stage, out var result)
                    ? result.AsEnumerable()
                    : Enumerable.Empty<StageConfiguration>();
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

                orderedStages.AddRange(Configuration(stage));

                visiting.Remove(stage);
            }

            void TopSort()
            {
                foreach (var stage in this.Configurations.Keys)
                    VisitStage(stage);
            }

            TopSort();
            return appBuilder =>
            {
                foreach (var stage in orderedStages)
                    stage.Configuration(appBuilder);
            };
        }

        public void AddConfiguration(string stage, Action<AspNetCoreConfigurationBuilder> configuration)
        {
            if (string.IsNullOrEmpty(stage)) throw new ArgumentException("message", nameof(stage));

            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            if (!this.Configurations.TryGetValue(stage, out var configurations))
                this.Configurations[stage] = configurations = new List<StageConfiguration>();

            configurations.Add(new StageConfiguration(stage, configuration));
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