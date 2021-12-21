using System;
using System.Collections.Generic;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator;
using kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator.Parts;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Tests.Validation.RelativityProviderValidator
{
    [TestFixture, Category("Unit")]
    public class RelativityProviderSourceProductionPermissionValidatorTests : TestBase
    {
        private IProductionRepository _productionRepository;
        private IAPILog _logger;
        private int _workspaceId = 111222;
        private int _productionId = 333444;

        public override void SetUp()
        {
            _productionRepository = Substitute.For<IProductionRepository>();
            _logger = Substitute.For<IAPILog>();
        }

        [Test]
        public void ShouldReturnSuccessfulValidationResultWhenThereIsNoAccessToProduction()
        {
            //arrange
            _productionRepository.GetProduction(_workspaceId, _productionId).Returns(new ProductionDTO() { ArtifactID = _productionId.ToString(), DisplayName = "Test Production"});
            RelativityProviderSourceProductionPermissionValidator validator = new RelativityProviderSourceProductionPermissionValidator(_productionRepository, _logger);

            //act
            var result = validator.Validate(_workspaceId, _productionId);

            //assert
            _productionRepository.Received(1).GetProduction(_workspaceId, _productionId);
            Assert.True(result.IsValid);
            Assert.False(result.Messages.Any());
        }

        [Test]
        public void ShouldReturnFailedValidationResultWhenThereIsNoAccessToProduction()
        {
            //arrange
            _productionRepository.GetProduction(_workspaceId, _productionId)
                .Throws(new Exception("Unable to retrieve production"));
            RelativityProviderSourceProductionPermissionValidator validator = new RelativityProviderSourceProductionPermissionValidator(_productionRepository, _logger);

            //act
            var result = validator.Validate(_workspaceId, _productionId);

            //assert
            _productionRepository.Received(1).GetProduction(_workspaceId, _productionId);

            Assert.False(result.IsValid);
            Assert.True(result.Messages.Count() == 1);
            Assert.True(result.Messages.First().ErrorCode == "20.007");
        }
    }
}
