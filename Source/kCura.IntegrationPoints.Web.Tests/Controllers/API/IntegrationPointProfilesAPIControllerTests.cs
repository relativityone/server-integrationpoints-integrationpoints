using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Helpers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Web.Controllers.API;
using kCura.IntegrationPoints.Web.Helpers;
using kCura.IntegrationPoints.Web.Models.Validation;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Relativity.API;
using Relativity.Telemetry.Services.Metrics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using kCura.IntegrationPoints.Web.Extensions;
using NSubstitute;

namespace kCura.IntegrationPoints.Web.Tests.Controllers.API
{
    [TestFixture, Category("Unit")]
    public class IntegrationPointProfilesAPIControllerTests : TestBase
    {
        private Mock<ICPHelper> _cpHelperFake;
        private Mock<IIntegrationPointService> _integrationPointServiceFake;
        private Mock<IIntegrationPointProfileService> _profileServiceFake;
        private Mock<IRelativityUrlHelper> _urlHelperFake;
        private Mock<IRelativityObjectManager> _objectManagerFake;
        private Mock<IValidationExecutor> _validationExecutorFake;
        private Mock<ICryptographyHelper> _cryptographyHelperFake;
        private Mock<IAPILog> _logFake;
        private Mock<IAPMManager> _apmManagerFake;

        private IntegrationPointProfilesAPIController _sut;

        private const int _WORKSPACE_ID = 23432;

        [SetUp]
        public override void SetUp()
        {
            _apmManagerFake = new Mock<IAPMManager>();

            Mock<IServicesMgr> svcMgrStub = new Mock<IServicesMgr>();
            svcMgrStub.Setup(m => m.CreateProxy<IAPMManager>(It.IsAny<ExecutionIdentity>()))
                .Returns(_apmManagerFake.Object);
            svcMgrStub.Setup(m => m.CreateProxy<IMetricsManager>(It.IsAny<ExecutionIdentity>()))
                .Returns(new Mock<IMetricsManager>().Object);

            _cpHelperFake = new Mock<ICPHelper>();
            _cpHelperFake.Setup(m => m.GetServicesManager()).Returns(svcMgrStub.Object);

            _integrationPointServiceFake = new Mock<IIntegrationPointService>();
            _profileServiceFake = new Mock<IIntegrationPointProfileService>();
            _urlHelperFake = new Mock<IRelativityUrlHelper>();
            _objectManagerFake = new Mock<IRelativityObjectManager>();
            _validationExecutorFake = new Mock<IValidationExecutor>();
            _cryptographyHelperFake = new Mock<ICryptographyHelper>();
            _logFake = new Mock<IAPILog>();

            _sut = new IntegrationPointProfilesAPIController(
                _cpHelperFake.Object,
                _profileServiceFake.Object,
                _integrationPointServiceFake.Object,
                _urlHelperFake.Object,
                _objectManagerFake.Object,
                _validationExecutorFake.Object,
                _cryptographyHelperFake.Object,
                _logFake.Object,
                CamelCaseSerializer)
            {
                Request = new HttpRequestMessage()
            };

            _sut.Request.SetConfiguration(new System.Web.Http.HttpConfiguration());
        }

        [Test]
        public void Save_ShouldReturnOk_WhenIntegrationPointProfilValidationSucceeded()
        {
            // arrange
            const int integrationPointProfileID = 100;
            var model = new IntegrationPointProfileDto()
            {
                Name = "Integration Point Test Profile"
            };

            _profileServiceFake.Setup(m => m.SaveProfile(model)).Returns(integrationPointProfileID);

            // act
            HttpResponseMessage response = _sut.Save(_WORKSPACE_ID, model.ToWebModel(CamelCaseSerializer));

            // assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Test]
        public void Save_ShouldReturnNotAcceptable_WhenIntegrationPointProfileValidationFails()
        {
            // arrange
            var model = new IntegrationPointProfileDto()
            {
                Name = "Integration Point Test Profile"
            };

            var errors = new List<string> { "Error1", "Error2" };
            var validationResult = new ValidationResult(errors);

            _profileServiceFake.Setup(m => m.SaveProfile(It.IsAny<IntegrationPointProfileDto>())).Throws(new IntegrationPointValidationException(validationResult));

            // act
            HttpResponseMessage response = _sut.Save(_WORKSPACE_ID, model.ToWebModel(CamelCaseSerializer));
            string responseContent = response.Content.ReadAsStringAsync().Result;
            ValidationResultDTO contentAsValidationResult = JsonConvert.DeserializeObject<ValidationResultDTO>(responseContent);

            // assert
            response.StatusCode.Should().Be(HttpStatusCode.NotAcceptable);
            contentAsValidationResult.IsValid.Should().BeFalse();
            contentAsValidationResult.Errors.Should().HaveCount(validationResult.Messages.Count());
        }

        [Test]
        public void SaveUsingIntegrationPoint_ShouldReturnNotAcceptable_WhenIntegrationPointValidationFails()
        {
            // arrange
            const string profileName = "Integration Point Test Profile";
            const int integrationPointArtifactID = 123123;

            _integrationPointServiceFake.Setup(m => m.Read(integrationPointArtifactID))
                .Returns(new IntegrationPointDto { ArtifactId = integrationPointArtifactID });

            var errors = new List<string> { "Error1", "Error2" };
            var validationResult = new ValidationResult(errors);

            _profileServiceFake.Setup(m => m.SaveProfile(It.IsAny<IntegrationPointProfileDto>()))
                .Throws(new IntegrationPointValidationException(validationResult));

            // act
            HttpResponseMessage response = _sut.SaveUsingIntegrationPoint(_WORKSPACE_ID, new IntegrationPointProfileFromIntegrationPointModel
            {
                IntegrationPointArtifactId = integrationPointArtifactID,
                ProfileName = profileName
            });
            string responseContent = response.Content.ReadAsStringAsync().Result;
            ValidationResultDTO contentAsValidationResult = JsonConvert.DeserializeObject<ValidationResultDTO>(responseContent);

            // assert
            response.StatusCode.Should().Be(HttpStatusCode.NotAcceptable);
            contentAsValidationResult.IsValid.Should().BeFalse();
            contentAsValidationResult.Errors.Should().HaveCount(validationResult.Messages.Count());
        }

        [Test]
        public void SaveUsingIntegrationPoint_ShouldReturnOkResponse_WhenProfileWasSavedAndCodeThrewAnError()
        {
            // Arrange
            const int integrationPointArtifactID = 123123;

            _integrationPointServiceFake.Setup(m => m.Read(integrationPointArtifactID))
                .Returns(new IntegrationPointDto { ArtifactId = integrationPointArtifactID });

            const int integrationPointProfileID = 100;
            const string profileName = "Integration Point Test Profile";

            _profileServiceFake.Setup(m => m.SaveProfile(It.IsAny<IntegrationPointProfileDto>()))
                .Returns(integrationPointProfileID);

            _apmManagerFake.Setup(x => x.Dispose()).Throws<Exception>();

            // Act
            HttpResponseMessage response = _sut.SaveUsingIntegrationPoint(_WORKSPACE_ID, new IntegrationPointProfileFromIntegrationPointModel
            {
                IntegrationPointArtifactId = integrationPointArtifactID,
                ProfileName = profileName
            });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
