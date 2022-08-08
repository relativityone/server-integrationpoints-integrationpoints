using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.IntegrationPoints.FieldsMapping.Metrics;
using Relativity.Services.Exceptions;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.IntegrationPoints.FieldsMapping.Tests
{
    [TestFixture]
    public class MetricBucketNameGeneratorTests
    {
        private Mock<IObjectManager> _objectManagerFake;

        private MetricBucketNameGenerator _sut;

        [SetUp]
        public void SetUp()
        {
            _objectManagerFake = new Mock<IObjectManager>();
            Mock<IServicesMgr>  servicesMgrFake = new Mock<IServicesMgr>();
            servicesMgrFake.Setup(x => x.CreateProxy<IObjectManager>(It.IsAny<ExecutionIdentity>())).Returns(_objectManagerFake.Object);
            Mock<IAPILog> loggerFake = new Mock<IAPILog>();

            _sut = new MetricBucketNameGenerator(servicesMgrFake.Object, loggerFake.Object);
        }

        [Test]
        public async Task GetBucketNameAsync_ShouldRemoveWhiteSpacesFromProviderName()
        {
            // Arrange
            const string metricName = "MetricName";
            const string providerName = "Some Provider";

            QueryResult queryResult = new QueryResult()
            {
                Objects = new List<RelativityObject>()
                {
                    new RelativityObject()
                    {
                        Name = providerName
                    }
                }
            };
            _objectManagerFake.Setup(x => x.QueryAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(queryResult);

            // Act
            string actual = await _sut.GetAutoMapBucketNameAsync(metricName, Guid.NewGuid(), 123).ConfigureAwait(false);

            // Assert
            const string expectedBucketName = "SomeProvider.AutoMap.MetricName";
            actual.Should().Be(expectedBucketName);
        }

        [Test]
        public async Task GetBucketNameAsync_ShouldReturnDefaultName_WhenExceptionIsThrown()
        {
            // Arrange
            const string metricName = "MetricName";
            _objectManagerFake.Setup(x => x.QueryAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>())).Throws<ServiceException>();

            // Act
            string actual = await _sut.GetAutoMapBucketNameAsync(metricName, Guid.NewGuid(), 123).ConfigureAwait(false);

            // Assert
            actual.Should().Be($"AutoMap.{metricName}");
        }
    }
}