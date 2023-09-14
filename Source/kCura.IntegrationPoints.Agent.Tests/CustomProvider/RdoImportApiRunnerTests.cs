﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Agent.CustomProvider;
using kCura.IntegrationPoints.Agent.CustomProvider.DTO;
using kCura.IntegrationPoints.Agent.CustomProvider.ImportStage;
using kCura.IntegrationPoints.Agent.CustomProvider.ImportStage.ImportApiService;
using kCura.IntegrationPoints.Agent.CustomProvider.ImportStage.RdoFlow;
using kCura.IntegrationPoints.Common.Kepler;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Agent.Tests.CustomProvider
{
    [TestFixture]
    [Category("Unit")]
    internal class RdoImportApiRunnerTests
    {
        private IFixture _fxt;
        private ImportJobContext _importJobContext;
        private RdoImportConfiguration _importConfiguration;

        private Mock<IRdoImportSettingsBuilder> _settingsBuilderMock;
        private Mock<IObjectManager> _objectManagerMock;
        private Mock<IKeplerServiceFactory> _keplerFactoryMock;
        private Mock<IImportApiService> _importApiServiceMock;
        private RdoImportApiRunner _sut;

        [SetUp]
        public void SetUp()
        {
            _fxt = FixtureFactory.Create();
            _importConfiguration = _fxt.Create<RdoImportConfiguration>();
            _importJobContext = _fxt.Create<ImportJobContext>();

            _importApiServiceMock = new Mock<IImportApiService>();
            _settingsBuilderMock = new Mock<IRdoImportSettingsBuilder>();
            _settingsBuilderMock
                .Setup(x => x.Build(It.IsAny<CustomProviderDestinationConfiguration>(), It.IsAny<List<IndexedFieldMap>>(), It.IsAny<IndexedFieldMap>()))
                .Returns(_importConfiguration);

            _objectManagerMock = new Mock<IObjectManager>();
            _objectManagerMock.Setup(x => x.QuerySlimAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), 0, 1))
                .ReturnsAsync(new QueryResultSlim
                {
                    Objects = new List<RelativityObjectSlim>
                    {
                        new RelativityObjectSlim
                        {
                            Values = new List<object>
                            {
                                new Random().Next()
                            }
                        }
                    },
                    ResultCount = 1,
                    TotalCount = 1
                });

            _keplerFactoryMock = new Mock<IKeplerServiceFactory>();
            _keplerFactoryMock
                .Setup(x => x.CreateProxyAsync<IObjectManager>())
                .ReturnsAsync(_objectManagerMock.Object);

            _sut = new RdoImportApiRunner(
                _settingsBuilderMock.Object,
                _importApiServiceMock.Object,
                new Mock<IAPILog>().Object);
        }

        [Test]
        public async Task RunImportJobAsync_ShouldCallSettingsBuilder()
        {
            // Arrange
            var integrationPointInfo = _fxt.Create<IntegrationPointInfo>();
            var identifier = _fxt.Create<IndexedFieldMap>();

            // Act
            await _sut.RunImportJobAsync(_importJobContext, integrationPointInfo, identifier);

            // Assert
            _settingsBuilderMock.Verify(x => x.Build(integrationPointInfo.DestinationConfiguration, integrationPointInfo.FieldMap, identifier), Times.Once);
        }

        [Test]
        public async Task RunImportJobAsync_ShouldCallImportApiService()
        {
            // Arrange
            var integrationPointInfo = _fxt.Create<IntegrationPointInfo>();
            var identifier = _fxt.Create<IndexedFieldMap>();

            // Act
            await _sut.RunImportJobAsync(_importJobContext, integrationPointInfo, identifier);

            // Assert
            _importApiServiceMock.Verify(x => x.CreateImportJobAsync(_importJobContext), Times.Once);
            _importApiServiceMock.Verify(x => x.ConfigureRdoImportApiJobAsync(_importJobContext, _importConfiguration), Times.Once);
            _importApiServiceMock.Verify(x => x.StartImportJobAsync(_importJobContext), Times.Once);
        }
    }
}
