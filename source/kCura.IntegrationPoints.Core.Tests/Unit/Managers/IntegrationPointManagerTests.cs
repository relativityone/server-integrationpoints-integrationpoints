using System;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.Relativity.Client;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Unit.Managers
{
	[Ignore]
	[TestFixture]
	public class IntegrationPointManagerTests
	{
		private IIntegrationPointManager _testInstance;
		private IRepositoryFactory _repositoryFactory;
		private IIntegrationPointRepository _integrationPointRepository;
		private ISourceProviderRepository _sourceProviderRepository;

		private const int WORKSPACE_ID = 100532;
		private const int INTEGRATION_POINT_ID = 101323; 

		[SetUp]
		public void Setup()
		{
			_repositoryFactory = Substitute.For<IRepositoryFactory>();
			_integrationPointRepository = Substitute.For<IIntegrationPointRepository>();
			_sourceProviderRepository = Substitute.For<ISourceProviderRepository>();
			_testInstance = new IntegrationPointManager(_repositoryFactory);
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
			Constants.SourceProvider sourceProvider = _testInstance.GetSourceProvider(WORKSPACE_ID, integrationPointDto);

			// Assert	
			Assert.AreEqual(isRelativityProvider ? Constants.SourceProvider.Relativity : Constants.SourceProvider.Other, sourceProvider);
		}

		[TestCase(true)]
		[TestCase(false)]
		public void UserHasPermissions_SourceProviderNotSupplied_MakesCallToCheck(bool isRelativityProvider)
		{
			IPermissionRepository permissionRepository = _repositoryFactory.GetPermissionRepository(WORKSPACE_ID);

			// Arrange
			int savedSearchArtifactId = 94902;
			var integrationPointDto = new IntegrationPointDTO()
			{
				SourceProvider = 123,
				SourceConfiguration = $"{{SavedSearchArtifactId: {savedSearchArtifactId}}}"
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

			permissionRepository.UserCanImport().Returns(true);
			permissionRepository.UserCanEditDocuments().Returns(true);
			permissionRepository.UserHasArtifactInstancePermission(Arg.Is((int)ArtifactType.Search), Arg.Is(savedSearchArtifactId), ArtifactPermission.View).Returns(true);

			// Act
			PermissionCheckDTO permissionCheckDto = _testInstance.UserHasPermissionToRunJob(WORKSPACE_ID, integrationPointDto, null);

			// Assert	
			permissionRepository.Received(1).UserCanImport();
			permissionRepository.Received(isRelativityProvider ? 1 : 0).UserCanEditDocuments();
			permissionRepository.Received(isRelativityProvider ? 1 : 0).UserHasArtifactInstancePermission(Arg.Is((int)ArtifactType.Search), Arg.Is(savedSearchArtifactId), ArtifactPermission.View);

			Assert.IsTrue(permissionCheckDto.Success);
			Assert.AreEqual(null, permissionCheckDto.ErrorMessages);
		}

		[Test]
		public void UserHasPermissions_SourceProviderNotSuppliedNoImportPermissions_ReturnsFalse()
		{
			IPermissionRepository permissionRepository = _repositoryFactory.GetPermissionRepository(WORKSPACE_ID);

			// Arrange
			int savedSearchArtifactId = 94902;
			var integrationPointDto = new IntegrationPointDTO()
			{
				SourceProvider = 123,
				SourceConfiguration = $"{{SavedSearchArtifactId: {savedSearchArtifactId}}}"
			};

			permissionRepository.UserCanImport().Returns(false);

			// Act
			PermissionCheckDTO permissionCheckDto = _testInstance.UserHasPermissionToRunJob(WORKSPACE_ID, integrationPointDto, null);

			// Assert	
			permissionRepository.Received(1).UserCanImport();
			permissionRepository.Received(0).UserCanEditDocuments();
			permissionRepository.Received(0).UserHasArtifactInstancePermission(Arg.Is((int)ArtifactType.Search), Arg.Is(savedSearchArtifactId), ArtifactPermission.View);

			Assert.IsFalse(permissionCheckDto.Success);
			Assert.AreEqual(Constants.IntegrationPoints.NO_PERMISSION_TO_IMPORT_CURRENTWORKSPACE, permissionCheckDto.ErrorMessages);
		}

		[Test]
		public void UserHasPermissions_NonRelativityProvider_UserHasAllPermissions_Success()
		{
			IPermissionRepository permissionRepository = _repositoryFactory.GetPermissionRepository(WORKSPACE_ID);

			// Arrange
			int savedSearchArtifactId = 94902;
			var integrationPointDto = new IntegrationPointDTO()
			{
				SourceProvider = 123,
				SourceConfiguration = $"{{SavedSearchArtifactId: {savedSearchArtifactId}}}"
			};

			permissionRepository.UserCanImport().Returns(true);

			// Act
			PermissionCheckDTO permissionCheckDto = _testInstance.UserHasPermissionToRunJob(WORKSPACE_ID, integrationPointDto, Core.Constants.SourceProvider.Other);

			// Assert	
			permissionRepository.Received(1).UserCanImport();
			permissionRepository.Received(0).UserCanEditDocuments();
			permissionRepository.Received(0).UserHasArtifactInstancePermission(Arg.Is((int)ArtifactType.Search), Arg.Is(savedSearchArtifactId), ArtifactPermission.View);

			Assert.IsTrue(permissionCheckDto.Success);
			Assert.AreEqual(null, permissionCheckDto.ErrorMessages);
		}

		[Test]
		public void UserHasPermissions_NonRelativityProvider_NoImportPermissions_Fails()
		{
			IPermissionRepository permissionRepository = _repositoryFactory.GetPermissionRepository(WORKSPACE_ID);

			// Arrange
			int savedSearchArtifactId = 94902;
			var integrationPointDto = new IntegrationPointDTO()
			{
				SourceProvider = 123,
				SourceConfiguration = $"{{SavedSearchArtifactId: {savedSearchArtifactId}}}"
			};

			permissionRepository.UserCanImport().Returns(false);

			// Act
			PermissionCheckDTO permissionCheckDto = _testInstance.UserHasPermissionToRunJob(WORKSPACE_ID, integrationPointDto, Core.Constants.SourceProvider.Other);

			// Assert	
			permissionRepository.Received(1).UserCanImport();
			permissionRepository.Received(0).UserCanEditDocuments();
			permissionRepository.Received(0).UserHasArtifactInstancePermission(Arg.Is((int)ArtifactType.Search), Arg.Is(savedSearchArtifactId), ArtifactPermission.View);

			Assert.IsFalse(permissionCheckDto.Success);
			Assert.AreEqual(Constants.IntegrationPoints.NO_PERMISSION_TO_IMPORT_CURRENTWORKSPACE, permissionCheckDto.ErrorMessages);
		}

		[Test]
		public void UserHasPermissions_RelativityProvider_UserHasAllPermissions_Success()
		{
			IPermissionRepository permissionRepository = _repositoryFactory.GetPermissionRepository(WORKSPACE_ID);

			// Arrange
			int savedSearchArtifactId = 94902;
			var integrationPointDto = new IntegrationPointDTO()
			{
				SourceProvider = 123,
				SourceConfiguration = $"{{SavedSearchArtifactId: {savedSearchArtifactId}}}"
			};

			permissionRepository.UserCanEditDocuments().Returns(true);
			permissionRepository.UserCanImport().Returns(true);
			permissionRepository.UserHasArtifactInstancePermission(Arg.Is((int) ArtifactType.Search), Arg.Is(savedSearchArtifactId), ArtifactPermission.View).Returns(true);

			// Act
			PermissionCheckDTO permissionCheckDto = _testInstance.UserHasPermissionToRunJob(WORKSPACE_ID, integrationPointDto, Core.Constants.SourceProvider.Relativity);

			// Assert	
			permissionRepository.Received(1).UserCanEditDocuments();
			permissionRepository.Received(1).UserCanImport();
			permissionRepository.Received(1).UserHasArtifactInstancePermission(Arg.Is((int)ArtifactType.Search), Arg.Is(savedSearchArtifactId), ArtifactPermission.View);

			Assert.IsTrue(permissionCheckDto.Success);
			Assert.AreEqual(null, permissionCheckDto.ErrorMessages);
		}

		[Test]
		public void UserHasPermissions_RelativityProvider_NoEditDocPermissions_Failure()
		{
			IPermissionRepository permissionRepository = _repositoryFactory.GetPermissionRepository(WORKSPACE_ID);

			// Arrange
			int savedSearchArtifactId = 94902;
			var integrationPointDto = new IntegrationPointDTO()
			{
				SourceProvider = 123,
				SourceConfiguration = $"{{SavedSearchArtifactId: {savedSearchArtifactId}}}"
			};

			permissionRepository.UserCanImport().Returns(true);
			permissionRepository.UserCanEditDocuments().Returns(false);

			// Act
			PermissionCheckDTO permissionCheckDto = _testInstance.UserHasPermissionToRunJob(WORKSPACE_ID, integrationPointDto, Core.Constants.SourceProvider.Relativity);

			// Assert	
			permissionRepository.Received(1).UserCanImport();
			permissionRepository.Received(1).UserCanEditDocuments();
			permissionRepository.Received(0).UserHasArtifactInstancePermission(Arg.Is((int)ArtifactType.Search), Arg.Is(savedSearchArtifactId), ArtifactPermission.View);

			Assert.IsFalse(permissionCheckDto.Success);
			Assert.AreEqual(Constants.IntegrationPoints.NO_PERMISSION_TO_EDIT_DOCUMENTS, permissionCheckDto.ErrorMessages);
		}

		[Test]
		public void UserHasPermissions_RelativityProvider_NoImportPermissions_Failure()
		{
			IPermissionRepository permissionRepository = _repositoryFactory.GetPermissionRepository(WORKSPACE_ID);

			// Arrange
			int savedSearchArtifactId = 94902;
			var integrationPointDto = new IntegrationPointDTO()
			{
				SourceProvider = 123,
				SourceConfiguration = $"{{SavedSearchArtifactId: {savedSearchArtifactId}}}"
			};

			permissionRepository.UserCanImport().Returns(false);

			// Act
			PermissionCheckDTO permissionCheckDto = _testInstance.UserHasPermissionToRunJob(WORKSPACE_ID, integrationPointDto, Core.Constants.SourceProvider.Relativity);

			// Assert	
			permissionRepository.Received(1).UserCanImport();
			permissionRepository.Received(0).UserCanEditDocuments();
			permissionRepository.Received(0).UserHasArtifactInstancePermission(Arg.Is((int)ArtifactType.Search), Arg.Is(savedSearchArtifactId), ArtifactPermission.View);

			Assert.IsFalse(permissionCheckDto.Success);
			Assert.AreEqual(Constants.IntegrationPoints.NO_PERMISSION_TO_IMPORT_CURRENTWORKSPACE, permissionCheckDto.ErrorMessages);
		}

		[Test]
		public void UserHasPermissions_RelativityProvider_NoSavedSearchPermissions_Failure()
		{
			IPermissionRepository permissionRepository = _repositoryFactory.GetPermissionRepository(WORKSPACE_ID);

			// Arrange
			int savedSearchArtifactId = 94902;
			var integrationPointDto = new IntegrationPointDTO()
			{
				SourceProvider = 123,
				SourceConfiguration = $"{{SavedSearchArtifactId: {savedSearchArtifactId}}}"
			};

			permissionRepository.UserCanImport().Returns(true);
			permissionRepository.UserCanEditDocuments().Returns(true);
			permissionRepository.UserHasArtifactInstancePermission(Arg.Is((int)ArtifactType.Search), Arg.Is(savedSearchArtifactId), ArtifactPermission.View).Returns(false);

			// Act
			PermissionCheckDTO permissionCheckDto = _testInstance.UserHasPermissionToRunJob(WORKSPACE_ID, integrationPointDto, Core.Constants.SourceProvider.Relativity);

			// Assert	
			permissionRepository.Received(1).UserCanImport();
			permissionRepository.Received(1).UserCanEditDocuments();
			permissionRepository.Received(1).UserHasArtifactInstancePermission(Arg.Is((int)ArtifactType.Search), Arg.Is(savedSearchArtifactId), ArtifactPermission.View);

			Assert.IsFalse(permissionCheckDto.Success);
			Assert.AreEqual(Constants.IntegrationPoints.NO_PERMISSION_TO_ACCESS_SAVEDSEARCH, permissionCheckDto.ErrorMessages);
		}
	}
}
