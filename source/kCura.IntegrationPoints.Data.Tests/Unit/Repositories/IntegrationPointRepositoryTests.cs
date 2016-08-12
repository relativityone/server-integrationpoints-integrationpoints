﻿using System.Collections.Generic;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.Data.Tests.Unit.Transformer;
using kCura.IntegrationPoints.Data.Transformers;
using kCura.IntegrationPoints.Domain.Models;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Data.Tests.Unit.Repositories
{
    [TestFixture]
    public class IntegrationPointRepositoryTests
    {
        private IIntegrationPointRepository _testInstance;
        private IGenericLibrary<IntegrationPoint> _integrationPointLibrary;
        private IDtoTransformer<IntegrationPointDTO, IntegrationPoint> _dtoTransformer;

        private const int INTEGRATION_POINT_ARTIFACT_ID_1 = 101400;
        private const int INTEGRATION_POINT_ARTIFACT_ID_2 = 101401;

        [OneTimeSetUp]
        public void Setup()
        {
            _integrationPointLibrary = Substitute.For<IGenericLibrary<IntegrationPoint>>();
            _dtoTransformer = Substitute.For<IDtoTransformer<IntegrationPointDTO, IntegrationPoint>>();
            _testInstance = new IntegrationPointRepository(_integrationPointLibrary, _dtoTransformer);
        }

        [Test]
        public void ReadSingleInstanceTest()
        {
            // ARRANGE
            IntegrationPoint integrationPoint = new IntegrationPointTransformerTests().CreateMockedIntegrationPoint("test integration point");
            _integrationPointLibrary.Read(INTEGRATION_POINT_ARTIFACT_ID_1).Returns(integrationPoint);
            IntegrationPointDTO integrationPointDto = new IntegrationPointDTO() {Name = integrationPoint.Name};
            _dtoTransformer.ConvertToDto(integrationPoint).Returns(integrationPointDto);

            // ACT
            IntegrationPointDTO resultDto = _testInstance.Read(INTEGRATION_POINT_ARTIFACT_ID_1);

            // ASSERT
            Assert.AreEqual(integrationPointDto, resultDto);
        }

        [Test]
        public void ReadMultiInstanceTest()
        {
            // ARRANGE
            int[] artifactIds = { INTEGRATION_POINT_ARTIFACT_ID_1, INTEGRATION_POINT_ARTIFACT_ID_2 };
            string expectedName1 = "My integration point name 1";
            string expectedName2 = "My integration point name 2";

            var helper = new IntegrationPointTransformerTests();
            var integrationPoints = new List<IntegrationPoint>()
            {
                helper.CreateMockedIntegrationPoint(expectedName1),
                helper.CreateMockedIntegrationPoint(expectedName2)
            };

            var expectedIntegrationPointDtos = new List<IntegrationPointDTO>()
            {
                {new IntegrationPointDTO() {Name = expectedName1}},
                {new IntegrationPointDTO() {Name = expectedName2}}
            };

            _integrationPointLibrary.Read(artifactIds).Returns(integrationPoints);
            _dtoTransformer.ConvertToDto(integrationPoints).Returns(expectedIntegrationPointDtos);


            // ACT
            List<IntegrationPointDTO> resultDtos = _testInstance.Read(artifactIds);

            // ASSERT
            Assert.AreEqual(expectedIntegrationPointDtos, resultDtos);
        }


    }
}
