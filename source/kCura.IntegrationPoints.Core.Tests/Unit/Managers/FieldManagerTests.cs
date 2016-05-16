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
	public class FieldManagerTests
	{
		private IFieldManager _testInstance;
		private IRepositoryFactory _repositoryFactory;
		private IFieldRepository _fieldRepository;

		private const int _WORKSPACE_ID = 100532;

		[SetUp]
		public void Setup()
		{
			_repositoryFactory = Substitute.For<IRepositoryFactory>();
			_fieldRepository = Substitute.For<IFieldRepository>();
			_testInstance = new FieldManager(_repositoryFactory);

			_repositoryFactory.GetFieldRepository(_WORKSPACE_ID).Returns(_fieldRepository);
		}

		[Test]
		public void RetrieveFieldArtifactIds_GoldFlow()
		{
			// ARRANGE
			var guids = new Guid[] {Guid.NewGuid(), Guid.NewGuid()};
			var expectedResult = new Dictionary<Guid, int>()
			{
				{guids[0], 123123},
				{guids[1], 324234}
			};
			
			_fieldRepository.RetrieveFieldArtifactIds(guids).Returns(expectedResult);

			// ACT
			Dictionary<Guid, int> result = _testInstance.RetrieveFieldArtifactIds(_WORKSPACE_ID, guids);

			// ASSERT
			Assert.AreEqual(expectedResult, result);
		}

		[Test]
		public void RetrieveArtifactViewFieldId_GoldFlow()
		{
			// ARRANGE
			int? expectedResult = 123123;
			int fieldArtifactId = 234242;

			_fieldRepository.RetrieveArtifactViewFieldId(fieldArtifactId).Returns(expectedResult);

			// ACT
			int? result = _testInstance.RetrieveArtifactViewFieldId(_WORKSPACE_ID, fieldArtifactId);

			// ASSERT
			Assert.AreEqual(expectedResult, result);
		}

	}
}
