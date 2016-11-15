using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Managers
{
	[TestFixture]
	public class FieldManagerTests : TestBase
	{
		private IFieldManager _testInstance;
		private IRepositoryFactory _repositoryFactory;
		private IFieldRepository _fieldRepository;

		private const int _WORKSPACE_ID = 100532;

		[SetUp]
		public override void SetUp()
		{
			_repositoryFactory = Substitute.For<IRepositoryFactory>();
			_fieldRepository = Substitute.For<IFieldRepository>();
			_testInstance = new FieldManager(_repositoryFactory);

			_repositoryFactory.GetFieldRepository(_WORKSPACE_ID).Returns(_fieldRepository);
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
