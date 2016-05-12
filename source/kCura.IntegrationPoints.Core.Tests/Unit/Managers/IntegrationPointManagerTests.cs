using System;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
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

		[TestFixtureSetUp]
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
		public void IntegrationPointTypeIsRetriable_GoldFlow(bool isRelativityProvider)
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
	}
}
