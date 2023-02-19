using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.Relativity.ImportAPI;
using Newtonsoft.Json;
using NSubstitute;
using NUnit.Framework;
using Relativity;
using Relativity.API;
using Relativity.IntegrationPoints.Contracts.Models;

namespace kCura.IntegrationPoints.DocumentTransferProvider.Tests
{
    [TestFixture, Category("Unit")]
    public class DocumentTransferProviderTests : TestBase
    {
        private const int _WORKSPACE_ARTIFACT_ID = 1111111;
        private const int _RDO_TYPE_ID = 9876;
        private static readonly string _documentTransferSettings = $"{{\"SourceWorkspaceArtifactId\":{_WORKSPACE_ARTIFACT_ID}}}";
        private readonly List<string> _excludedFieldNames = new List<string>
        {
            Domain.Constants.SPECIAL_SOURCEWORKSPACE_FIELD_NAME,
            Domain.Constants.SPECIAL_SOURCEJOB_FIELD_NAME,
            DocumentFields.RelativityDestinationCase,
            DocumentFields.JobHistory
        };

        private List<ArtifactDTO> _retrievieFieldsResult;
        private List<Relativity.ImportAPI.Data.Field> _getWorkspaceFieldsResult = new List<Relativity.ImportAPI.Data.Field>();
        private readonly List<int> _expectedArtifactIds = new List<int> {123, 321, 333};

        private readonly List<int> _excludedArtifactIds = new List<int> {987, 789, 673};

        [SetUp]
        public override void SetUp()
        {
        }

        [Test]
        public void GetEmailBodyData_HasWorkspace_CorrectlyFormatedOutput()
        {
            // ARRANGE
            var repositoryFactory = Substitute.For<IRepositoryFactory>();
            var workspaceRepository = Substitute.For<IWorkspaceRepository>();
            var workspace = new WorkspaceDTO { ArtifactId = _WORKSPACE_ARTIFACT_ID, Name = "My Test workspace" };
            repositoryFactory.GetWorkspaceRepository().Returns(workspaceRepository);
            workspaceRepository.Retrieve(Arg.Any<int>()).Returns(workspace);

            var mockDocumentTransferProvider = new DocumentTransferProvider(Substitute.For<IImportApiFacade>(), repositoryFactory, Substitute.For<IAPILog>());

            var settings = new DocumentTransferSettings { SourceWorkspaceArtifactId = _WORKSPACE_ARTIFACT_ID };
            var options = JsonConvert.SerializeObject(settings);

            // ACT
            var returnedString = mockDocumentTransferProvider.GetEmailBodyData(null, options);

            // ASSERT
            Assert.AreEqual("\r\nSource Workspace: My Test workspace - 1111111", returnedString);
        }

        [Test]
        public void GetEmailBodyData_NoWorkspace_CorrectlyFormatedOutput()
        {
            var repositoryFactory = Substitute.For<IRepositoryFactory>();
            var workspaceRepository = Substitute.For<IWorkspaceRepository>();
            WorkspaceDTO workspace = null;
            repositoryFactory.GetWorkspaceRepository().Returns(workspaceRepository);
            workspaceRepository.Retrieve(Arg.Any<int>()).Returns(workspace);

            var mockDocumentTransferProvider = new DocumentTransferProvider(Substitute.For<IImportApiFacade>(), Substitute.For<IRepositoryFactory>(), Substitute.For<IAPILog>());
            var settings = new DocumentTransferSettings { SourceWorkspaceArtifactId = _WORKSPACE_ARTIFACT_ID };
            var options = JsonConvert.SerializeObject(settings);

            // ACT
            var returnedString = mockDocumentTransferProvider.GetEmailBodyData(null, options);

            // ASSERT
            Assert.AreEqual("", returnedString);
        }

        [Test]
        public void GetFields_GoldFlow()
        {
            _getWorkspaceFieldsResult = PrepareGetWorkspaceFieldsResult();
            _retrievieFieldsResult = PrepareRetrieveFieldsResult(_getWorkspaceFieldsResult);
            _getWorkspaceFieldsResult.AddRange(PrepareFieldsToBeExcluded());

            var documentTransferProvider = new DocumentTransferProvider(GetImportApiMock(), GetRepositoryFactoryMock(), Substitute.For<IAPILog>());

            IEnumerable<FieldEntry> documentFields = documentTransferProvider.GetFields(new DataSourceProviderConfiguration(_documentTransferSettings)).ToList();

            Assert.IsTrue(documentFields.All(documentField => !_excludedFieldNames.Contains(documentField.DisplayName)));
            Assert.IsTrue(documentFields.All(documentDield => _expectedArtifactIds.Any(id => id.ToString() == documentDield.FieldIdentifier)));
        }

        private IRepositoryFactory GetRepositoryFactoryMock()
        {
            IFieldQueryRepository fieldQueryRepository = Substitute.For<IFieldQueryRepository>();
            fieldQueryRepository.RetrieveFields(_RDO_TYPE_ID, Arg.Any<HashSet<string>>())
                .Returns(_retrievieFieldsResult.ToArray());
            IRepositoryFactory repositoryFactory = Substitute.For<IRepositoryFactory>();
            repositoryFactory.GetFieldQueryRepository(_WORKSPACE_ARTIFACT_ID).Returns(fieldQueryRepository);
            return repositoryFactory;
        }

        private IImportApiFacade GetImportApiMock()
        {
            IImportAPI importApi = Substitute.For<IImportAPI>();
            importApi.GetWorkspaceFields(_WORKSPACE_ARTIFACT_ID, (int) ArtifactType.Field).Returns(_getWorkspaceFieldsResult);
            IImportApiFactory importApiFactory = Substitute.For<IImportApiFactory>();
            importApiFactory.Create().Returns(importApi);
            var logger = Substitute.For<IAPILog>();
            IImportApiFacade importApiFacade = new ImportApiFacade(importApiFactory, logger);
            return importApiFacade;
        }

        private List<Relativity.ImportAPI.Data.Field> PrepareGetWorkspaceFieldsResult()
        {
            List<Relativity.ImportAPI.Data.Field> list = _excludedFieldNames.Select(fileName => CreateField(fileName.GetHashCode(), fileName)).ToList();
            list.AddRange(_expectedArtifactIds.Select(id => CreateField(id, $"TestField {id}")));
            return list;
        }

        private IEnumerable<Relativity.ImportAPI.Data.Field> PrepareFieldsToBeExcluded()
        {
            return _excludedArtifactIds.Select(id => CreateField(id, $"FieldToExclude {id}"));
        }

        private List<ArtifactDTO> PrepareRetrieveFieldsResult(List<Relativity.ImportAPI.Data.Field> preparedWorkspaceFields)
        {
            return preparedWorkspaceFields.Select(
                field => new ArtifactDTO(field.ArtifactID, (int) ArtifactType.Field, "", new List<ArtifactFieldDTO>())).ToList();
        }

        private Relativity.ImportAPI.Data.Field CreateField(int artifactID, string name)
        {
            var field = new Relativity.ImportAPI.Data.Field();
            field.GetType().GetProperty(nameof(Relativity.ImportAPI.Data.Field.ArtifactID)).SetValue(field, artifactID, null);
            field.GetType().GetProperty(nameof(Relativity.ImportAPI.Data.Field.Name)).SetValue(field, name, null);

            return field;
        }
    }
}
