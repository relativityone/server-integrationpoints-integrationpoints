using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Managers
{
    [TestFixture, Category("Unit")]
    public class FieldManagerTests : TestBase
    {
        private IFieldManager _testInstance;
        private IRepositoryFactory _repositoryFactory;
        private IFieldQueryRepository _fieldQueryRepository;
        private ArtifactFieldDTO[] _fieldArray;

        private const int _WORKSPACE_ID = 100532;

        [SetUp]
        public override void SetUp()
        {
            _repositoryFactory = Substitute.For<IRepositoryFactory>();
            _fieldQueryRepository = Substitute.For<IFieldQueryRepository>();
            _testInstance = new FieldManager(_repositoryFactory);

            _repositoryFactory.GetFieldQueryRepository(_WORKSPACE_ID).Returns(_fieldQueryRepository);
        }

        [Test]
        public void RetrieveArtifactViewFieldId_GoldFlow()
        {
            // ARRANGE
            int? expectedResult = 123123;
            int fieldArtifactId = 234242;

            _fieldQueryRepository.RetrieveArtifactViewFieldId(fieldArtifactId).Returns(expectedResult);

            // ACT
            int? result = _testInstance.RetrieveArtifactViewFieldId(_WORKSPACE_ID, fieldArtifactId);

            // ASSERT
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void RetrieveBeginBatesFields()
        {
            GenerateFieldList();
            // ARRANGE
            ArtifactFieldDTO[] expectedResult = _fieldArray;

            _fieldQueryRepository.RetrieveBeginBatesFields().Returns(expectedResult);

            // ACT
            ArtifactFieldDTO[] result = _testInstance.RetrieveBeginBatesFields(_WORKSPACE_ID);

            // ASSERT
            CollectionAssert.AreEqual(expectedResult, result);
        }

        private void GenerateFieldList()
        {
            List<ArtifactFieldDTO> fieldList = new List<ArtifactFieldDTO>();
            for (int i = 1; i < 6; i++)
            {
                fieldList.Add(new ArtifactFieldDTO { ArtifactId = i, Name = string.Format("FieldName{0}", i.ToString()) });
            }

            _fieldArray = fieldList.ToArray();
        }

    }
}
