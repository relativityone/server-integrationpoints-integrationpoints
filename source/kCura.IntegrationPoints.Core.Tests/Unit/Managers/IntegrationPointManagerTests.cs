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

		private const int WORKSPACE_ID = 100532;
		private const int INTEGRATION_POINT_ID = 101323; 

		[TestFixtureSetUp]
		public void Setup()
		{
			_repositoryFactory = Substitute.For<IRepositoryFactory>();
			_integrationPointRepository = Substitute.For<IIntegrationPointRepository>();
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
	}
}
