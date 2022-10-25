using System;
using Autofac;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.Integration.Helpers;

namespace Relativity.Sync.Tests.Integration
{
    internal class DocumentSynchronizationMonitorExecutorTests
    {
        [Test]
        public void DocumentSynchronizationMonitorExecutor_ShouldBeResolved()
        {
            // Arrange
            ContainerBuilder containerBuilder = ContainerHelper.CreateInitializedContainerBuilder();
            IntegrationTestsContainerBuilder.MockStepsExcept<IDocumentSynchronizationMonitorConfiguration>(containerBuilder);

            IContainer container = containerBuilder.Build();

            // Act
            Action resolve = () => container.Resolve<IExecutor<IDocumentSynchronizationMonitorConfiguration>>();

            // Assert
            resolve.Should().NotThrow();
        }

    }
}
