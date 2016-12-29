using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Managers
{
	[TestFixture]
	public class IntegrationPointManagerTests : TestBase
	{
		private IIntegrationPointManager _testInstance;
		private IRepositoryFactory _repositoryFactory;
		private IIntegrationPointRepository _integrationPointRepository;

		private const int _SOURCE_WORKSPACE_ID = 100532;
		private const int INTEGRATION_POINT_ID = 101323;

		[SetUp]
		public override void SetUp()
		{
			_repositoryFactory = Substitute.For<IRepositoryFactory>();
			_integrationPointRepository = Substitute.For<IIntegrationPointRepository>();
			_testInstance = new IntegrationPointManager(_repositoryFactory);
			_repositoryFactory.GetIntegrationPointRepository(_SOURCE_WORKSPACE_ID).Returns(_integrationPointRepository);
		}

		[Test]
		public void ReadTest()
		{
			// ARRANGE
			string expectedName = "MyTest";
			var expectedIntegrationPointDto = new IntegrationPointDTO() {Name = expectedName};

			_integrationPointRepository.Read(INTEGRATION_POINT_ID).Returns(expectedIntegrationPointDto);

			// ACT
			IntegrationPointDTO dto = _testInstance.Read(_SOURCE_WORKSPACE_ID, INTEGRATION_POINT_ID);

			// ASSERT
			Assert.IsNotNull(dto);
			Assert.AreEqual(expectedName, dto.Name);
		}
	}
}
