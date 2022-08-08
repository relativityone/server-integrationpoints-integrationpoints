using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.EventHandlers.Commands.Context;
using kCura.IntegrationPoints.EventHandlers.Commands.Helpers;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers.Tests.Commands.Helpers
{
    [TestFixture, Category("Unit")]
    public class ArtifactsToDeleteTests : TestBase
    {
        private const string _SQL = "SELECT ArtifactID FROM {0}.[{1}]";
        private const int _WORKSPACE_ID = 889943;
        private const string _TEMP_TABLE_NAME_WITH_PARENT_ARTIFACTS_TO_DELETE = "temp_table_name";
        private const string _DB_PREPEND = "prefix";

        private ArtifactsToDelete _instance;
        private IDBContext _dbContext;

        public override void SetUp()
        {
            _dbContext = Substitute.For<IDBContext>();

            IScratchTableRepository scratchTableRepository = Substitute.For<IScratchTableRepository>();
            scratchTableRepository.GetResourceDBPrepend().Returns(_DB_PREPEND);

            IEHHelper helper = Substitute.For<IEHHelper>();
            helper.GetActiveCaseID().Returns(_WORKSPACE_ID);
            helper.GetDBContext(_WORKSPACE_ID).Returns(_dbContext);

            IEHContext context = new EHContext
            {
                Helper = helper,
                TempTableNameWithParentArtifactsToDelete = _TEMP_TABLE_NAME_WITH_PARENT_ARTIFACTS_TO_DELETE
            };

            IRepositoryFactory repositoryFactory = Substitute.For<IRepositoryFactory>();
            repositoryFactory.GetScratchTableRepository(_WORKSPACE_ID, string.Empty, string.Empty).Returns(scratchTableRepository);

            _instance = new ArtifactsToDelete(context, repositoryFactory);
        }

        [Test]
        public void ItShouldReturnArtifactIds()
        {
            var artifactIds = new List<int> {353899, 661463, 911317};
            DbDataReader reader = new DataTableReader(CreateDataTable(artifactIds));

            _dbContext.ExecuteSqlStatementAsDbDataReader(string.Format(_SQL, _DB_PREPEND, _TEMP_TABLE_NAME_WITH_PARENT_ARTIFACTS_TO_DELETE)).Returns(reader);

            // ACT
            var result = _instance.GetIds();

            // ASSERT
            CollectionAssert.AreEquivalent(artifactIds, result);
        }

        private DataTable CreateDataTable(List<int> list)
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add("c1", typeof(int));
            foreach (var i in list)
            {
                var row = dataTable.NewRow();
                row[0] = i;
                dataTable.Rows.Add(row);
            }
            return dataTable;
        }
    }
}