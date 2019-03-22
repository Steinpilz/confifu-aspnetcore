namespace Confifu.AspNetCore.Builder
{
    using System;
    using Microsoft.AspNetCore.Http.Features;

    class FeatureScope<TFeature> : IDisposable
    {
        readonly IFeatureCollection featureCollection;
        readonly TFeature oldFeature;

        public FeatureScope(IFeatureCollection featureCollection, TFeature feature)
        {
            this.featureCollection = featureCollection;
            this.oldFeature = featureCollection.Get<TFeature>();
            featureCollection.Set(feature);
        }

        public void Dispose()
        {
            this.featureCollection.Set(this.oldFeature);
        }
    }
}