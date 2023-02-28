using System;
using System.Linq;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using NSubstitute;
using NUnit.Framework;
using Relativity;

namespace kCura.IntegrationPoints.Data.Tests.Repositories.Implementations
{
    [TestFixture, Category("Unit")]
    public class QueryFieldLookupRepositoryTests
    {
        private IQueryFieldLookup _queryFieldLookup;
        private IExternalServiceSimpleInstrumentation _instrumentation;
        private IExternalServiceInstrumentationProvider _instrumentationProvider;

        [SetUp]
        public void SetUp()
        {
            _queryFieldLookup = Substitute.For<IQueryFieldLookup>();
            _instrumentation = Substitute.For<IExternalServiceSimpleInstrumentation>();
            _instrumentationProvider = Substitute.For<IExternalServiceInstrumentationProvider>();
            _instrumentationProvider.CreateSimple("Relativity.Data", "IQueryFieldLookup", "GetFieldByArtifactID").Returns(_instrumentation);
        }

        [Test]
        public void ItShouldReturnAViewFieldInfoObject()
        {
            // Arrange
            const int fieldArtifactId = 1000000;
            var expectedViewFieldInfo = new ViewFieldInfo("ColumnName", "DataSource", FieldTypeHelper.FieldType.Boolean);
            QueryFieldLookupRepository queryRepository = Substitute.For<QueryFieldLookupRepository>(_queryFieldLookup, _instrumentationProvider);
            queryRepository.RunQueryForViewFieldInfo(Arg.Any<int>()).Returns(expectedViewFieldInfo);

            // Act
            ViewFieldInfo returnedFieldInfoObject = queryRepository.GetFieldByArtifactID(fieldArtifactId);

            // Assert
            queryRepository.Received(1).RunQueryForViewFieldInfo(fieldArtifactId);
            Assert.AreEqual(expectedViewFieldInfo.FieldType, returnedFieldInfoObject.FieldType);
            Assert.AreEqual(expectedViewFieldInfo.DisplayName, returnedFieldInfoObject.DisplayName);
            Assert.AreEqual(expectedViewFieldInfo.DataSource, returnedFieldInfoObject.DataSource);
        }

        [Test]
        public void ItShouldCacheAViewFieldInfoObject()
        {
            // Arrange
            const int fieldArtifactId = 1000000;
            var expectedViewFieldInfo = new ViewFieldInfo("ColumnName", "DataSource", FieldTypeHelper.FieldType.Boolean);
            QueryFieldLookupRepository queryRepository = Substitute.For<QueryFieldLookupRepository>(_queryFieldLookup, _instrumentationProvider);
            queryRepository.RunQueryForViewFieldInfo(Arg.Any<int>()).Returns(expectedViewFieldInfo);

            // Act
            queryRepository.GetFieldByArtifactID(fieldArtifactId);

            // Assert
            queryRepository.Received(1).RunQueryForViewFieldInfo(fieldArtifactId);
            Assert.Contains(fieldArtifactId, queryRepository.ViewFieldsInfoCache.Keys);
            Assert.AreEqual(expectedViewFieldInfo.FieldType, queryRepository.ViewFieldsInfoCache.Values.First().FieldType);
            Assert.AreEqual(expectedViewFieldInfo.DisplayName, queryRepository.ViewFieldsInfoCache.Values.First().DisplayName);
            Assert.AreEqual(expectedViewFieldInfo.DataSource, queryRepository.ViewFieldsInfoCache.Values.First().DataSource);
        }

        [Test]
        public void ItShouldHaveOneObjectCached()
        {
            // Arrange
            const int fieldArtifactId = 1000000;
            var expectedViewFieldInfo = new ViewFieldInfo("ColumnName", "DataSource", FieldTypeHelper.FieldType.Boolean);
            QueryFieldLookupRepository queryRepository = Substitute.For<QueryFieldLookupRepository>(_queryFieldLookup, _instrumentationProvider);
            queryRepository.RunQueryForViewFieldInfo(Arg.Any<int>()).Returns(expectedViewFieldInfo);

            // PreAct check
            Assert.AreEqual(0, queryRepository.ViewFieldsInfoCache.Count, "There should be 0 items cached.");

            // Act - multiple queries for the same object
            queryRepository.GetFieldByArtifactID(fieldArtifactId);
            queryRepository.GetFieldByArtifactID(fieldArtifactId);
            queryRepository.GetFieldByArtifactID(fieldArtifactId);
            queryRepository.GetFieldByArtifactID(fieldArtifactId);

            // Assert
            queryRepository.Received(1).RunQueryForViewFieldInfo(fieldArtifactId);
            Assert.AreEqual(1, queryRepository.ViewFieldsInfoCache.Count, "There should be 1 items cached.");
        }

        [Test]
        public void ItShouldHaveTwoObjectsCached()
        {
            // Arrange
            const int field1ArtifactId = 1000000;
            const int field2ArtifactId = 2000000;
            var expectedViewFieldInfo = new ViewFieldInfo("ColumnName", "DataSource", FieldTypeHelper.FieldType.Boolean);
            QueryFieldLookupRepository queryRepository = Substitute.For<QueryFieldLookupRepository>(_queryFieldLookup, _instrumentationProvider);
            queryRepository.RunQueryForViewFieldInfo(Arg.Any<int>()).Returns(expectedViewFieldInfo);

            // PreAct check
            Assert.AreEqual(0, queryRepository.ViewFieldsInfoCache.Count, "There should be 0 items cached.");

            // Act - multiple queries for the same object
            queryRepository.GetFieldByArtifactID(field1ArtifactId);
            queryRepository.GetFieldByArtifactID(field2ArtifactId);

            // Assert
            queryRepository.Received(2).RunQueryForViewFieldInfo(Arg.Any<int>());
            Assert.AreEqual(2, queryRepository.ViewFieldsInfoCache.Count, "There should be 2 items cached.");
            Assert.Contains(field1ArtifactId, queryRepository.ViewFieldsInfoCache.Keys);
            Assert.Contains(field2ArtifactId, queryRepository.ViewFieldsInfoCache.Keys);
        }

        [Test]
        public void ItShouldCallQueryFieldLookupAndInstrumentationWhenGettingField()
        {
            // Arrange
            const int fieldArtifactId = 1000001;
            var expectedViewFieldInfo = new ViewFieldInfo("ColumnName", "DataSource", FieldTypeHelper.FieldType.Boolean);
            _queryFieldLookup.GetFieldByArtifactID(fieldArtifactId).Returns(expectedViewFieldInfo);
            _instrumentation.Execute(Arg.Any<Func<ViewFieldInfo>>()).Returns(c => c.ArgAt<Func<ViewFieldInfo>>(0).Invoke());
            IQueryFieldLookupRepository queryRepository = new QueryFieldLookupRepository(_queryFieldLookup, _instrumentationProvider);

            // Act
            ViewFieldInfo actualResult = queryRepository.GetFieldByArtifactID(fieldArtifactId);

            // Assert
            _queryFieldLookup.Received(1).GetFieldByArtifactID(fieldArtifactId);
            _instrumentationProvider.Received(1).CreateSimple("Relativity.Data", "IQueryFieldLookup", "GetFieldByArtifactID");
            _instrumentation.Received(1).Execute(Arg.Any<Func<ViewFieldInfo>>());
            Assert.AreEqual(expectedViewFieldInfo, actualResult);
        }

        [Test]
        public void ItShouldCallQueryFieldLookupAndInstrumentationWhenGettingFieldType()
        {
            // Arrange
            const int fieldArtifactID = 1000002;
            var expectedViewFieldInfo = new ViewFieldInfo("ColumnName", "DataSource", FieldTypeHelper.FieldType.Boolean);
            _queryFieldLookup.GetFieldByArtifactID(fieldArtifactID).Returns(expectedViewFieldInfo);
            _instrumentation.Execute(Arg.Any<Func<ViewFieldInfo>>()).Returns(c => c.ArgAt<Func<ViewFieldInfo>>(0).Invoke());
            IQueryFieldLookupRepository queryRepository = new QueryFieldLookupRepository(_queryFieldLookup, _instrumentationProvider);

            // Act
            FieldTypeHelper.FieldType actualResult = queryRepository.GetFieldTypeByArtifactID(fieldArtifactID);

            // Assert
            _queryFieldLookup.Received(1).GetFieldByArtifactID(fieldArtifactID);
            _instrumentationProvider.Received(1).CreateSimple("Relativity.Data", "IQueryFieldLookup", "GetFieldByArtifactID");
            _instrumentation.Received(1).Execute(Arg.Any<Func<ViewFieldInfo>>());
            Assert.AreEqual(expectedViewFieldInfo.FieldType, actualResult);
        }
    }
}
