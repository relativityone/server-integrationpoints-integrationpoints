using System;
using System.Collections.Generic;
using Castle.Core;
using Castle.MicroKernel;

namespace kCura.IntegrationPoint.Tests.Core.TestHelpers
{
    public class WindsorDependenciesGraphRecorder
    {
        private readonly HashSet<KeyValuePair<Type, Type>> dependenciesGraph = new HashSet<KeyValuePair<Type, Type>>();

        public WindsorDependenciesGraphRecorder(IKernel kernel)
        {
            kernel.DependencyResolving += OnDependencyResolving;
        }

        public bool WasDependencyPresent<TParent, TChild>()
        {
            var expectedDependencyNode = new KeyValuePair<Type, Type>(typeof(TParent), typeof(TChild));
            return dependenciesGraph.Contains(expectedDependencyNode);
        }

        private void OnDependencyResolving(ComponentModel client, DependencyModel model, object dependency)
        {
            var dependencyNode = new KeyValuePair<Type, Type>(client.Implementation, dependency.GetType());
            dependenciesGraph.Add(dependencyNode);
        }
    }
}
