using System;
using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using Relativity.Logging;

namespace kCura.IntegrationPoints.Services.Tests
{
    [TestFixture]
    public class IntegrationPointManagerLoggingTests : ServiceTestsBase
    {
        [Test]
        public void ItShouldLogErrorFromCreateIntegrationPointAsync()
        {
            // arrange
            var manager = new IntegrationPointManager(Logger, PermissionRepositoryFactory);
            var request = new CreateIntegrationPointRequest();

            // act & assert
            Assert.Throws<AggregateException>(() =>
            {
                IntegrationPointModel actual = manager.CreateIntegrationPointAsync(request).Result;
            });

            Logger.Received().LogError(Arg.Any<Exception>(), Arg.Any<string>(), Arg.Any<object[]>());
        }

        [Test]
        public void ItShouldLogErrorFromUpdateIntegrationPointAsync()
        {
            // arrange
            var manager = new IntegrationPointManager(Logger, PermissionRepositoryFactory);
            var request = new UpdateIntegrationPointRequest();

            // act & assert
            Assert.Throws<AggregateException>(() =>
            {
                IntegrationPointModel actual = manager.UpdateIntegrationPointAsync(request).Result;
            });

            Logger.Received().LogError(Arg.Any<Exception>(), Arg.Any<string>(), Arg.Any<object[]>());
        }

        [Test]
        public void ItShouldLogErrorFromGetIntegrationPointAsync()
        {
            // arrange
            var manager = new IntegrationPointManager(Logger, PermissionRepositoryFactory);

            // act & assert
            Assert.Throws<AggregateException>(() =>
            {
                IntegrationPointModel actual = manager.GetIntegrationPointAsync(0, 0).Result;
            });

            Logger.Received().LogError(Arg.Any<Exception>(), Arg.Any<string>(), Arg.Any<object[]>());
        }

        [Test]
        public void ItShouldLogErrorFromRunIntegrationPointAsync()
        {
            // arrange
            var manager = new IntegrationPointManager(Logger, PermissionRepositoryFactory);

            // act & assert
            Assert.Throws<AggregateException>(() =>
            {
                manager.RunIntegrationPointAsync(0, 0).Wait();
            });

            Logger.Received().LogError(Arg.Any<Exception>(), Arg.Any<string>(), Arg.Any<object[]>());
        }

        [Test]
        public void ItShouldLogErrorFromGetAllIntegrationPointsAsync()
        {
            // arrange
            var manager = new IntegrationPointManager(Logger, PermissionRepositoryFactory);

            // act & assert
            Assert.Throws<AggregateException>(() =>
            {
                IList<IntegrationPointModel> actual = manager.GetAllIntegrationPointsAsync(0).Result;
            });

            Logger.Received().LogError(Arg.Any<Exception>(), Arg.Any<string>(), Arg.Any<object[]>());
        }

        [Test]
        public void ItShouldLogErrorFromGetSourceProviderArtifactIdAsync()
        {
            // arrange
            var manager = new IntegrationPointManager(Logger, PermissionRepositoryFactory);

            // act & assert
            Assert.Throws<AggregateException>(() =>
            {
                int actual = manager.GetSourceProviderArtifactIdAsync(0, string.Empty).Result;
            });

            Logger.Received().LogError(Arg.Any<Exception>(), Arg.Any<string>(), Arg.Any<object[]>());
        }

        [Test]
        public void ItShouldLogErrorFromGetIntegrationPointArtifactTypeIdAsync()
        {
            // arrange
            var manager = new IntegrationPointManager(Logger, PermissionRepositoryFactory);

            // act & assert
            Assert.Throws<AggregateException>(() =>
            {
                int actual = manager.GetIntegrationPointArtifactTypeIdAsync(0).Result;
            });

            Logger.Received().LogError(Arg.Any<Exception>(), Arg.Any<string>(), Arg.Any<object[]>());
        }
    }
}