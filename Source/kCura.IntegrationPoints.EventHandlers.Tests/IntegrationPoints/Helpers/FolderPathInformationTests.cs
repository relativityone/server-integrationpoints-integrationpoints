using System.Data;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Data.DbContext;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Newtonsoft.Json;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers.Tests.IntegrationPoints.Helpers
{
    [TestFixture, Category("Unit")]
    public class FolderPathInformationTests : TestBase
    {
        public override void SetUp()
        {
            _dbContext = Substitute.For<IWorkspaceDBContext>();
            _instace = new FolderPathInformation(_dbContext);
        }

        private IWorkspaceDBContext _dbContext;
        private FolderPathInformation _instace;

        [TestCase(ImportOverwriteModeEnum.AppendOnly, false)]
        [TestCase(ImportOverwriteModeEnum.AppendOverlay, false)]
        [TestCase(ImportOverwriteModeEnum.AppendOverlay, true)]
        [TestCase(ImportOverwriteModeEnum.OverlayOnly, false)]
        [TestCase(ImportOverwriteModeEnum.OverlayOnly, true)]
        public void ItShouldReturnEmptyString(ImportOverwriteModeEnum overwriteMode, bool useFolderPathInfo)
        {
            var settings = CreateSettings(overwriteMode, useFolderPathInfo, 1);

            var result = _instace.RetrieveName(settings);

            Assert.That(result, Is.Empty);

            _dbContext.Received(0).ExecuteSqlStatementAsDataTable(Arg.Any<string>());
        }

        private string CreateSettings(ImportOverwriteModeEnum overwriteMode, bool useFolderPathInfo, int folderPathSourceField)
        {
            FolderPathInformation.IntegrationPointDestinationConfiguration settings = new FolderPathInformation.IntegrationPointDestinationConfiguration
            {
                FolderPathSourceField = folderPathSourceField,
                ImportOverwriteMode = overwriteMode,
                UseFolderPathInformation = useFolderPathInfo
            };
            return JsonConvert.SerializeObject(settings);
        }

        [Test]
        public void ItShouldReturnTextIdentifier()
        {
            string expectedTextIdentifier = "expected_text_identifier_249";

            int folderPathSourceField = 843;
            ImportOverwriteModeEnum overwriteMode = ImportOverwriteModeEnum.AppendOnly;
            bool useFolderPathInfo = true;

            var dataTable = new DataTable();
            dataTable.Columns.Add("Column1");
            var row = dataTable.NewRow();
            row["Column1"] = expectedTextIdentifier;
            dataTable.Rows.Add(row);

            _dbContext.ExecuteSqlStatementAsDataTable($"SELECT TextIdentifier FROM Artifact WHERE ArtifactID = {folderPathSourceField}").Returns(dataTable);

            var settings = CreateSettings(overwriteMode, useFolderPathInfo, folderPathSourceField);

            var result = _instace.RetrieveName(settings);

            Assert.That(result, Is.EqualTo(expectedTextIdentifier));
        }
    }
}
