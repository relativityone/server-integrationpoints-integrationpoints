using System;
using System.Collections.Generic;
using System.Data;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using NSubstitute;
using NUnit.Framework;
using Relativity.Core;

namespace kCura.IntegrationPoints.Data.Tests.Repositories
{
    [TestFixture]
    public class SqlArtifactGuidRepositoryTests : TestBase
    {
        private IArtifactGuidRepository _testInstance;
        private BaseContext _context;
        private kCura.Data.RowDataGateway.BaseContext _dBContext;

        [SetUp]
        public override void SetUp()
        {
            _context = Substitute.For<BaseContext>();
            _dBContext = Substitute.For<kCura.Data.RowDataGateway.BaseContext>();
            _context.DBContext.Returns(_dBContext);

            _testInstance = new SqlArtifactGuidRepository(_context);
        }

        [Test]
        public void GetGuidsForArtifactIds_GoldFlow()
        {
            // ARRANGE
            int artifactId1 = 123456;
            int artifactId2 = 987654;
            Guid[] guids = new Guid[]
            {
                new Guid("0C453BAF-DAF9-4793-BBC8-8DA03DF38EBC"), 
                new Guid("BBDB8C62-B068-4F28-B626-B68D0146D4BC"), 
            };

            int[] artifactIds = new [] { artifactId1, artifactId2 };
            string sql = $"SELECT [ArtifactID],[ArtifactGuid] FROM [eddsdbo].[ArtifactGuid] WITH (NOLOCK) WHERE [ArtifactID] IN ({artifactId1},{artifactId2})";

            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("ArtifactID", typeof(int));
            dataTable.Columns.Add("ArtifactGuid", typeof(object));
            dataTable.Rows.Add(artifactIds[0], guids[0]);
            dataTable.Rows.Add(artifactIds[1], guids[1]);

            _dBContext.ExecuteSqlStatementAsDataTable(sql).Returns(dataTable);

            // ACT
            Dictionary<int, Guid> result = _testInstance.GetGuidsForArtifactIds(artifactIds);

            // ASSERT
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(guids[0], result[artifactId1]);
            Assert.AreEqual(guids[1], result[artifactId2]);
        }

		[Test]
		public void GetArtifactIdsForGuids_GoldFlow()
		{
			// ARRANGE
			int artifactId1 = 123456;
			int artifactId2 = 987654;
			Guid guid1 = new Guid("0C453BAF-DAF9-4793-BBC8-8DA03DF38EBC");
			Guid guid2 = new Guid("BBDB8C62-B068-4F28-B626-B68D0146D4BC");

			var guids = new Guid[] { guid1, guid2 };
			int[] artifactIds = new[] { 123456, 987654 };

			string sql = $"SELECT [ArtifactGuid], [ArtifactID] FROM [eddsdbo].[ArtifactGuid] WITH (NOLOCK) WHERE [ArtifactGuid] IN ('{guid1}','{guid2}')";

			DataTable dataTable = new DataTable();
			dataTable.Columns.Add("ArtifactGuid", typeof(object));
			dataTable.Columns.Add("ArtifactID", typeof(int));
			
			dataTable.Rows.Add(guids[0], artifactIds[0]);
			dataTable.Rows.Add(guids[1], artifactIds[1]);

			_dBContext.ExecuteSqlStatementAsDataTable(sql).Returns(dataTable);

			// ACT
			Dictionary<Guid, int> result = _testInstance.GetArtifactIdsForGuids(guids);

			// ASSERT
			Assert.IsNotNull(result);
			Assert.AreEqual(2, result.Count);
			Assert.AreEqual(artifactId1, result[guids[0]]);
			Assert.AreEqual(artifactId2, result[guids[1]]);
		}


	}
}
