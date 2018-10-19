using kCura.IntegrationPoints.Data.Repositories.Implementations;
using NSubstitute;
using NUnit.Framework;
using Relativity;
using Relativity.Core;
using System.Linq;

namespace kCura.IntegrationPoints.Data.Tests.Repositories
{
	public class QueryFieldLookupRepositoryTests
	{
		private ICoreContext _context;

		[SetUp]
		public void SetUp()
		{
			_context = Substitute.For<ICoreContext>();
		}

		[Test]
		public void ItShouldReturnAViewFieldInfoObject()
		{
			//Arrange 
			const int fieldArtifactId = 1000000;
			var expectedViewFieldInfo = new ViewFieldInfo("ColumnName", "DataSource", FieldTypeHelper.FieldType.Boolean);
			var extendedViewFieldInfo = new ViewFieldInfoFieldTypeExtender(expectedViewFieldInfo);
			var queryRepository = Substitute.For<QueryFieldLookupRepository>(_context);
			queryRepository.RunQueryForViewFieldInfo(Arg.Any<int>()).Returns(extendedViewFieldInfo);
			
			//Act 
			ViewFieldInfo returnedFieldInfoObject = queryRepository.GetFieldByArtifactId(fieldArtifactId);

			//Assert
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
			var queryRepository = Substitute.For<QueryFieldLookupRepository>(_context);
			queryRepository.RunQueryForViewFieldInfo(Arg.Any<int>()).Returns(extendedViewFieldInfo);

			//Act 
			queryRepository.GetFieldByArtifactId(fieldArtifactId);

			//Assert
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
			var queryRepository = Substitute.For<QueryFieldLookupRepository>(_context);
			queryRepository.RunQueryForViewFieldInfo(Arg.Any<int>()).Returns(extendedViewFieldInfo);

			//PreAct check
			Assert.AreEqual(0, queryRepository.ViewFieldsInfoCache.Count, "There should be 0 items cached.");

			//Act - multiple queries for the same object
			queryRepository.GetFieldByArtifactId(fieldArtifactId);
			queryRepository.GetFieldByArtifactId(fieldArtifactId);
			queryRepository.GetFieldByArtifactId(fieldArtifactId);
			queryRepository.GetFieldByArtifactId(fieldArtifactId);

			//Assert
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
			var queryRepository = Substitute.For<QueryFieldLookupRepository>(_context);
			queryRepository.RunQueryForViewFieldInfo(Arg.Any<int>()).Returns(extendedViewFieldInfo);

			//PreAct check
			Assert.AreEqual(0, queryRepository.ViewFieldsInfoCache.Count, "There should be 0 items cached.");

			//Act - multiple queries for the same object
			queryRepository.GetFieldByArtifactId(field1ArtifactId);
			queryRepository.GetFieldByArtifactId(field2ArtifactId);

			//Assert
			Assert.AreEqual(2, queryRepository.ViewFieldsInfoCache.Count, "There should be 2 items cached.");
			Assert.Contains(field1ArtifactId, queryRepository.ViewFieldsInfoCache.Keys);
			Assert.Contains(field2ArtifactId, queryRepository.ViewFieldsInfoCache.Keys);
		}
	}
}
