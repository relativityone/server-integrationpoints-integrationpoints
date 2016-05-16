using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Unit.Managers
{
	[TestFixture]
	public class JobHistoryManagerTests
	{
		private IJobHistoryManager _testInstance;
		private IRepositoryFactory _repositoryFactory;
		private IJobHistoryRepository _jobHistoryRepository;

		private const int _WORKSPACE_ID = 100532;

		[TestFixtureSetUp]
		public void Setup()
		{
			_repositoryFactory = Substitute.For<IRepositoryFactory>();
			_jobHistoryRepository = Substitute.For<IJobHistoryRepository>();
			_testInstance = new JobHistoryManager(_repositoryFactory);

			_repositoryFactory.GetJobHistoryRepository(_WORKSPACE_ID).Returns(_jobHistoryRepository);
		}

		[Test]
		public void GetLastJobHistoryArtifactIdTests()
		{
			// ARRANGE
			int integrationPointArtifactId = 1322131;
			int expectedLastTwoJobHistoryIds = 234242;
			_jobHistoryRepository.GetLastJobHistoryArtifactId(integrationPointArtifactId).Returns(expectedLastTwoJobHistoryIds);

			// ACT
			int result = _testInstance.GetLastJobHistoryArtifactId(_WORKSPACE_ID, integrationPointArtifactId);

			// ASSERT
			Assert.AreEqual(expectedLastTwoJobHistoryIds, result);
		}


	}

}
