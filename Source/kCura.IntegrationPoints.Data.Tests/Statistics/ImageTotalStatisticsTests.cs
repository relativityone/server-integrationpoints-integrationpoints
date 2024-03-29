﻿using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Statistics.Implementations;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.DataContracts.DTOs.Results;
using Relativity.Services.Field;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Tests.Statistics
{
    [TestFixture, Category("Unit")]
    public class ImageTotalStatisticsTests : ImageStatisticsTestBase
    {
        private ImageTotalStatistics _instance;
        private IExportQueryResult _exportResult;

        public override void SetUp()
        {
            base.SetUp();
            _exportResult = Substitute.For<IExportQueryResult>();
            _relativityObjectManager.QueryWithExportAsync(Arg.Any<QueryRequest>(), Arg.Any<int>(), Arg.Any<ExecutionIdentity>())
                .Returns(_exportResult);
            _helper.GetLoggerFactory().GetLogger().ForContext<ImageTotalStatistics>().Returns(_logger);
            _instance = new ImageTotalStatistics(_helper, _repositoryFactory);
        }

        [Test]
        public void ItShouldReturnResultForFolder()
        {
            int expectedResult = 554;

            int folderId = 114438;
            int viewId = 398415;
            bool includeSubfolders = true;

            ConfigureExportResult(expectedResult, DocumentFieldsConstants.RelativityImageCountGuid);

            var actualResult = _instance.ForFolder(_WORKSPACE_ID, folderId, viewId, includeSubfolders);

            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [Test]
        public void ItShouldReturnResultForSavedSearch()
        {
            int expectedResult = 879;

            int savedSearchId = 768974;

            ConfigureExportResult(expectedResult, DocumentFieldsConstants.RelativityImageCountGuid);

            var actualResult = _instance.ForSavedSearch(_WORKSPACE_ID, savedSearchId);

            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [Test]
        public void ItShouldReturnResultForProduction()
        {
            int expectedResult = 424;

            int productionId = 998788;

            ConfigureExportResult(expectedResult, ProductionConsts.ImageCountFieldGuid);

            var actualResult = _instance.ForProduction(_WORKSPACE_ID, productionId);

            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        private void ConfigureExportResult(int expectedResult, Guid fieldGuid)
        {
            _exportResult.ExportResult.Returns(new ExportInitializationResults()
            {
                FieldData = new List<FieldMetadata>
                {
                    new FieldMetadata
                    {
                        Guids = new List<Guid> {fieldGuid}
                    }
                }
            });

            _exportResult.GetNextBlockAsync(0).Returns(new List<RelativityObjectSlim>
            {
                new RelativityObjectSlim
                {
                    Values = new List<object>
                    {
                        expectedResult
                    }
                }
            });
        }

        [Test]
        public void ItShouldLogError()
        {
            _relativityObjectManager.QueryWithExportAsync(Arg.Any<QueryRequest>(), Arg.Any<int>(), Arg.Any<ExecutionIdentity>())
                .Throws(new Exception());

            Assert.That(() => _instance.ForFolder(_WORKSPACE_ID, 157, 237, true), Throws.Exception);
            Assert.That(() => _instance.ForProduction(_WORKSPACE_ID, 465), Throws.Exception);
            Assert.That(() => _instance.ForSavedSearch(_WORKSPACE_ID, 740), Throws.Exception);

            _logger.Received(3).LogError(Arg.Any<Exception>(), Arg.Any<string>(), Arg.Any<object[]>());
        }
    }
}
