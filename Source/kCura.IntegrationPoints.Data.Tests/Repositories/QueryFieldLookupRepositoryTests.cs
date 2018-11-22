using System;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using NSubstitute;
using NUnit.Framework;
using Relativity;
using System.Linq;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;
using kCura.IntegrationPoints.Data.Repositories;

namespace kCura.IntegrationPoints.Data.Tests.Repositories
{
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
			_instrumentationProvider.CreateSimple("Relativity.Data", "QueryFieldLookup", "GetFieldByArtifactID").Returns(_instrumentation);
		}

		[Test]
		public void ItShouldReturnAViewFieldInfoObject()
		{
			//Arrange 
			const int fieldArtifactId = 1000000;
			var expectedViewFieldInfo = new ViewFieldInfo("ColumnName", "DataSource", FieldTypeHelper.FieldType.Boolean);
			var extendedViewFieldInfo = new ViewFieldInfoFieldTypeExtender(expectedViewFieldInfo);
			QueryFieldLookupRepository queryRepository = Substitute.For<QueryFieldLookupRepository>(_queryFieldLookup, _instrumentationProvider);
			queryRepository.RunQueryForViewFieldInfo(Arg.Any<int>()).Returns(extendedViewFieldInfo);
			
			//Act 
			ViewFieldInfo returnedFieldInfoObject = queryRepository.GetFieldByArtifactId(fieldArtifactId);

			//Assert
			queryRepository.Received(1).RunQueryForViewFieldInfo(fieldArtifactId);
			Assert.AreEqual(expectedViewFieldInfo.FieldType, returnedFieldInfoObject.FieldType);
			Assert.AreEqual(expectedViewFieldInfo.DisplayName, returnedFieldInfoObject.DisplayName);
			Assert.AreEqual(expectedViewFieldInfo.DataSource, returnedFieldInfoObject.DataSource);
		}

		[Test]
		public void ItShouldCacheAViewFieldInfoObject()
		{
			//Arrange 
			const int fieldArtifactId = 1000000;
			var expectedViewFieldInfo = new ViewFieldInfo("ColumnName", "DataSource", FieldTypeHelper.FieldType.Boolean);
			var extendedViewFieldInfo = new ViewFieldInfoFieldTypeExtender(expectedViewFieldInfo);
			QueryFieldLookupRepository queryRepository = Substitute.For<QueryFieldLookupRepository>(_queryFieldLookup, _instrumentationProvider);
			queryRepository.RunQueryForViewFieldInfo(Arg.Any<int>()).Returns(extendedViewFieldInfo);

			//Act 
			queryRepository.GetFieldByArtifactId(fieldArtifactId);

			//Assert
			queryRepository.Received(1).RunQueryForViewFieldInfo(fieldArtifactId);
			Assert.Contains(fieldArtifactId, queryRepository.ViewFieldsInfoCache.Keys);
			Assert.AreEqual(expectedViewFieldInfo.FieldType, queryRepository.ViewFieldsInfoCache.Values.First().Value.FieldType);
			Assert.AreEqual(expectedViewFieldInfo.DisplayName, queryRepository.ViewFieldsInfoCache.Values.First().Value.DisplayName);
			Assert.AreEqual(expectedViewFieldInfo.DataSource, queryRepository.ViewFieldsInfoCache.Values.First().Value.DataSource);
		}

		[Test]
		public void ItShouldHaveOneObjectCached()
		{
			//Arrange 
			const int fieldArtifactId = 1000000;
			var expectedViewFieldInfo = new ViewFieldInfo("ColumnName", "DataSource", FieldTypeHelper.FieldType.Boolean);
			var extendedViewFieldInfo = new ViewFieldInfoFieldTypeExtender(expectedViewFieldInfo);
			QueryFieldLookupRepository queryRepository = Substitute.For<QueryFieldLookupRepository>(_queryFieldLookup, _instrumentationProvider);
			queryRepository.RunQueryForViewFieldInfo(Arg.Any<int>()).Returns(extendedViewFieldInfo);

			//PreAct check
			Assert.AreEqual(0, queryRepository.ViewFieldsInfoCache.Count, "There should be 0 items cached.");

			//Act - multiple queries for the same object
			queryRepository.GetFieldByArtifactId(fieldArtifactId);
			queryRepository.GetFieldByArtifactId(fieldArtifactId);
			queryRepository.GetFieldByArtifactId(fieldArtifactId);
			queryRepository.GetFieldByArtifactId(fieldArtifactId);

			//Assert
			queryRepository.Received(1).RunQueryForViewFieldInfo(fieldArtifactId);
			Assert.AreEqual(1, queryRepository.ViewFieldsInfoCache.Count, "There should be 1 items cached.");
		}

		[Test]
		public void ItShouldHaveTwoObjectsCached()
		{
			//Arrange 
			const int field1ArtifactId = 1000000;
			const int field2ArtifactId = 2000000;
			var expectedViewFieldInfo = new ViewFieldInfo("ColumnName", "DataSource", FieldTypeHelper.FieldType.Boolean);
			var extendedViewFieldInfo = new ViewFieldInfoFieldTypeExtender(expectedViewFieldInfo);
			QueryFieldLookupRepository queryRepository = Substitute.For<QueryFieldLookupRepository>(_queryFieldLookup, _instrumentationProvider);
			queryRepository.RunQueryForViewFieldInfo(Arg.Any<int>()).Returns(extendedViewFieldInfo);

			//PreAct check
			Assert.AreEqual(0, queryRepository.ViewFieldsInfoCache.Count, "There should be 0 items cached.");

			//Act - multiple queries for the same object
			queryRepository.GetFieldByArtifactId(field1ArtifactId);
			queryRepository.GetFieldByArtifactId(field2ArtifactId);

			//Assert
			queryRepository.Received(2).RunQueryForViewFieldInfo(Arg.Any<int>());
			Assert.AreEqual(2, queryRepository.ViewFieldsInfoCache.Count, "There should be 2 items cached.");
			Assert.Contains(field1ArtifactId, queryRepository.ViewFieldsInfoCache.Keys);
			Assert.Contains(field2ArtifactId, queryRepository.ViewFieldsInfoCache.Keys);
		}

		[Test]
		public void ItShouldCallQueryFieldLookupAndInstrumentationWhenGettingField()
		{
			//Arrange
			const int fieldArtifactId = 1000001;
			var expectedResult = new ViewFieldInfo();
			_queryFieldLookup.GetFieldByArtifactID(fieldArtifactId).Returns(expectedResult);
			_instrumentation.Execute(Arg.Any<Func<ViewFieldInfo>>()).Returns(c => c.ArgAt<Func<ViewFieldInfo>>(0).Invoke());
			IQueryFieldLookupRepository queryRepository = new QueryFieldLookupRepository(_queryFieldLookup, _instrumentationProvider);
			
			//Act
			ViewFieldInfo actualResult = queryRepository.GetFieldByArtifactId(fieldArtifactId);

			//Assert
			_queryFieldLookup.Received(1).GetFieldByArtifactID(fieldArtifactId);
			_instrumentationProvider.Received(1).CreateSimple("Relativity.Data", "QueryFieldLookup", "GetFieldByArtifactID");
			_instrumentation.Received(1).Execute(Arg.Any<Func<ViewFieldInfo>>());
			Assert.AreEqual(expectedResult, actualResult);
		}

		[Test]
		public void ItShouldCallQueryFieldLookupAndInstrumentationWhenGettingFieldType()
		{
			//Arrange
			const int fieldArtifactId = 1000002;
			var expectedResult = new ViewFieldInfo();
			_queryFieldLookup.GetFieldByArtifactID(fieldArtifactId).Returns(expectedResult);
			_instrumentation.Execute(Arg.Any<Func<ViewFieldInfo>>()).Returns(c => c.ArgAt<Func<ViewFieldInfo>>(0).Invoke());
			IQueryFieldLookupRepository queryRepository = new QueryFieldLookupRepository(_queryFieldLookup, _instrumentationProvider);

			//Act
			string actualResult = queryRepository.GetFieldTypeByArtifactId(fieldArtifactId);

			//Assert
			_queryFieldLookup.Received(1).GetFieldByArtifactID(fieldArtifactId);
			_instrumentationProvider.Received(1).CreateSimple("Relativity.Data", "QueryFieldLookup", "GetFieldByArtifactID");
			_instrumentation.Received(1).Execute(Arg.Any<Func<ViewFieldInfo>>());
			Assert.AreEqual(expectedResult.FieldType.ToString(), actualResult);
		}
	}
}
