using System;
using ContentAnalyst.Web.AutomationService;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.Relativity.Client;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Unit.Managers
{
	[TestFixture]
	public class IntegrationPointManagerTests
	{
		private IIntegrationPointManager _testInstance;
		private IRepositoryFactory _repositoryFactory;
		private IIntegrationPointRepository _integrationPointRepository;
		private ISourceProviderRepository _sourceProviderRepository;
		private IPermissionRepository _permissionRepository;

		private const int WORKSPACE_ID = 100532;
		private const int INTEGRATION_POINT_ID = 101323; 

		[SetUp]
		public void Setup()
		{
			_repositoryFactory = Substitute.For<IRepositoryFactory>();
			_integrationPointRepository = Substitute.For<IIntegrationPointRepository>();
			_sourceProviderRepository = Substitute.For<ISourceProviderRepository>();
			_permissionRepository = Substitute.For<IPermissionRepository>();
			_testInstance = new IntegrationPointManager(_repositoryFactory, _permissionRepository);
		}

		[Test]
		public void ReadTest()
		{
			// ARRANGE
			string expectedName = "MyTest";
			var expectedIntegrationPointDto = new IntegrationPointDTO() {Name = expectedName };

			_repositoryFactory.GetIntegrationPointRepository(WORKSPACE_ID).Returns(_integrationPointRepository);
			_integrationPointRepository.Read(INTEGRATION_POINT_ID).Returns(expectedIntegrationPointDto);

			// ACT
			IntegrationPointDTO dto = _testInstance.Read(WORKSPACE_ID, INTEGRATION_POINT_ID);

			// ASSERT
			Assert.IsNotNull(dto);
			Assert.AreEqual(expectedName, dto.Name);
		}

		[TestCase(true)]
		[TestCase(false)]
		public void IntegrationPointSourceProviderIsRelativity_GoldFlow(bool isRelativityProvider)
		{
			// Arrange
			var integrationPointDto = new IntegrationPointDTO()
			{
				SourceProvider = 123
			};

			var sourceProviderDto = new SourceProviderDTO()
			{
				Name = "DOESN'T ACTUALLY MATTER",
				Identifier = isRelativityProvider
					? new Guid(Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID)
					: Guid.NewGuid()
			};
			_repositoryFactory.GetSourceProviderRepository(Arg.Is(WORKSPACE_ID))
				.Returns(_sourceProviderRepository);

			_sourceProviderRepository.Read(Arg.Is(integrationPointDto.SourceProvider.Value))
				.Returns(sourceProviderDto);

			// Act
			bool isRetriable = _testInstance.IntegrationPointSourceProviderIsRelativity(WORKSPACE_ID, integrationPointDto);

			// Assert	
			Assert.AreEqual(isRelativityProvider, isRetriable);
		}

		[Test]
		public void UserHasPermissions_UserHasAllPermissions_Success()
		{
			// Arrange
			int savedSearchArtifactId = 94902;
			var integrationPointDto = new IntegrationPointDTO()
			{
				SourceProvider = 123,
				SourceConfiguration = $"{{SavedSearchArtifactId: {savedSearchArtifactId}}}"
			};

			_permissionRepository.UserCanEditDocuments(WORKSPACE_ID).Returns(true);
			_permissionRepository.UserCanImport(WORKSPACE_ID).Returns(true);
			_permissionRepository.UserCanViewArtifact(Arg.Is(WORKSPACE_ID), Arg.Is((int) ArtifactType.Search), Arg.Is(savedSearchArtifactId)).Returns(true);

			// Act
			PermissionCheckDTO permissionCheckDto = _testInstance.UserHasPermissions(WORKSPACE_ID, integrationPointDto);

			// Assert	
			_permissionRepository.Received(1).UserCanEditDocuments(WORKSPACE_ID);
			_permissionRepository.Received(1).UserCanImport(WORKSPACE_ID);
			_permissionRepository.Received(1).UserCanViewArtifact(Arg.Is(WORKSPACE_ID), Arg.Is((int)ArtifactType.Search), Arg.Is(savedSearchArtifactId));

			Assert.IsTrue(permissionCheckDto.Success);
			Assert.AreEqual(null, permissionCheckDto.ErrorMessage);
		}

		[Test]
		public void UserHasPermissions_NoEditDocPermissions_Failure()
		{
			// Arrange
			int savedSearchArtifactId = 94902;
			var integrationPointDto = new IntegrationPointDTO()
			{
				SourceProvider = 123,
				SourceConfiguration = $"{{SavedSearchArtifactId: {savedSearchArtifactId}}}"
			};

			_permissionRepository.UserCanEditDocuments(WORKSPACE_ID).Returns(false);

			// Act
			PermissionCheckDTO permissionCheckDto = _testInstance.UserHasPermissions(WORKSPACE_ID, integrationPointDto);

			// Assert	
			_permissionRepository.Received(1).UserCanEditDocuments(WORKSPACE_ID);
			_permissionRepository.Received(0).UserCanImport(WORKSPACE_ID);
			_permissionRepository.Received(0).UserCanViewArtifact(Arg.Is(WORKSPACE_ID), Arg.Is((int)ArtifactType.Search), Arg.Is(savedSearchArtifactId));

			Assert.IsFalse(permissionCheckDto.Success);
			Assert.AreEqual(Constants.IntegrationPoints.NO_PERMISSION_TO_EDIT_DOCUMENTS, permissionCheckDto.ErrorMessage);
		}

		[Test]
		public void UserHasPermissions_NoImportPermissions_Failure()
		{
			// Arrange
			int savedSearchArtifactId = 94902;
			var integrationPointDto = new IntegrationPointDTO()
			{
				SourceProvider = 123,
				SourceConfiguration = $"{{SavedSearchArtifactId: {savedSearchArtifactId}}}"
			};

			_permissionRepository.UserCanEditDocuments(WORKSPACE_ID).Returns(true);
			_permissionRepository.UserCanImport(WORKSPACE_ID).Returns(false);

			// Act
			PermissionCheckDTO permissionCheckDto = _testInstance.UserHasPermissions(WORKSPACE_ID, integrationPointDto);

			// Assert	
			_permissionRepository.Received(1).UserCanEditDocuments(WORKSPACE_ID);
			_permissionRepository.Received(1).UserCanImport(WORKSPACE_ID);
			_permissionRepository.Received(0).UserCanViewArtifact(Arg.Is(WORKSPACE_ID), Arg.Is((int)ArtifactType.Search), Arg.Is(savedSearchArtifactId));

			Assert.IsFalse(permissionCheckDto.Success);
			Assert.AreEqual(Constants.IntegrationPoints.NO_PERMISSION_TO_IMPORT, permissionCheckDto.ErrorMessage);
		}

		[Test]
		public void UserHasPermissions_NoSavedSearchPermissions_Failure()
		{
			// Arrange
			int savedSearchArtifactId = 94902;
			var integrationPointDto = new IntegrationPointDTO()
			{
				SourceProvider = 123,
				SourceConfiguration = $"{{SavedSearchArtifactId: {savedSearchArtifactId}}}"
			};

			_permissionRepository.UserCanEditDocuments(WORKSPACE_ID).Returns(true);
			_permissionRepository.UserCanImport(WORKSPACE_ID).Returns(true);
			_permissionRepository.UserCanViewArtifact(Arg.Is(WORKSPACE_ID), Arg.Is((int)ArtifactType.Search), Arg.Is(savedSearchArtifactId)).Returns(false);

			// Act
			PermissionCheckDTO permissionCheckDto = _testInstance.UserHasPermissions(WORKSPACE_ID, integrationPointDto);

			// Assert	
			_permissionRepository.Received(1).UserCanEditDocuments(WORKSPACE_ID);
			_permissionRepository.Received(1).UserCanImport(WORKSPACE_ID);
			_permissionRepository.Received(1).UserCanViewArtifact(Arg.Is(WORKSPACE_ID), Arg.Is((int)ArtifactType.Search), Arg.Is(savedSearchArtifactId));

			Assert.IsFalse(permissionCheckDto.Success);
			Assert.AreEqual(Constants.IntegrationPoints.NO_PERMISSION_TO_ACCESS_SAVEDSEARCH, permissionCheckDto.ErrorMessage);
		}
	}
}
