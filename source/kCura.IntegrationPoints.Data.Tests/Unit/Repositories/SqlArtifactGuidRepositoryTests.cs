﻿using System;
using System.Collections.Generic;
using System.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using NSubstitute;
using NUnit.Framework;
using Relativity.Core;

namespace kCura.IntegrationPoints.Data.Tests.Unit.Repositories
{
    [TestFixture]
    public class SqlArtifactGuidRepositoryTests
    {
        private IArtifactGuidRepository _testInstance;
        private BaseContext _context;
        private kCura.Data.RowDataGateway.BaseContext _dBContext;

        [SetUp]
        public void Setup()
        {
            _context = Substitute.For<BaseContext>();
            _dBContext = Substitute.For<kCura.Data.RowDataGateway.BaseContext>();
            _context.DBContext.Returns(_dBContext);

            _testInstance = new SqlArtifactGuidRepository(_context);
        }

        [Test]
        public void InsertArtifactGuidsForArtifactIdsTest()
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
            string sql = $"SELECT [ArtifactID],[ArtifactGuid] FROM [eddsdbo].[ArtifactGuid] WHERE [ArtifactID] IN ({artifactId1},{artifactId2})";

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

    }
}
