using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.Synchronizer;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.IntegrationPoints.Web.Controllers.API;
using kCura.IntegrationPoints.Web.Extensions;
using kCura.IntegrationPoints.Web.Helpers;
using kCura.IntegrationPoints.Web.Models.Validation;
using Moq;
using Newtonsoft.Json;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Relativity.API;
using Relativity.Telemetry.Services.Metrics;
using static kCura.IntegrationPoints.Web.Controllers.API.IntegrationPointsAPIController;

namespace kCura.IntegrationPoints.Web.Tests.Controllers.API
{
    [TestFixture, Category("Unit")]
    public class IntegrationPointsAPIControllerTests : TestBase
    {
        private Mock<IAPILog> _loggerFake;
        private IntegrationPointsAPIController _sut;
        private IIntegrationPointService _integrationPointService;
        private IRelativityUrlHelper _relativityUrlHelper;
        private IRdoSynchronizerProvider _rdoSynchronizerProvider;
        private ICPHelper _cpHelper;
        private IServicesMgr _svcMgr;
        private const int _WORKSPACE_ID = 23432;
        private const string _CREDENTIALS = "{}";

        [SetUp]
        public override void SetUp()
        {
            _relativityUrlHelper = Substitute.For<IRelativityUrlHelper>();
            _integrationPointService = Substitute.For<IIntegrationPointService>();
            _rdoSynchronizerProvider = Substitute.For<IRdoSynchronizerProvider>();
            _cpHelper = Substitute.For<ICPHelper>();
            _svcMgr = Substitute.For<IServicesMgr>();

            _cpHelper.GetServicesManager().Returns(_svcMgr);
            _svcMgr.CreateProxy<IMetricsManager>(Arg.Any<ExecutionIdentity>())
                .Returns(Substitute.For<IMetricsManager>());

            _loggerFake = new Mock<IAPILog>();

            _sut = new IntegrationPointsAPIController(
                _relativityUrlHelper,
                _rdoSynchronizerProvider,
                _integrationPointService,
                CamelCaseSerializer,
                _loggerFake.Object)
            {
                Request = new HttpRequestMessage()
            };

            _sut.Request.SetConfiguration(new HttpConfiguration());
        }

        [TestCase(null)]
        [TestCase(1000)]
        public void Update_StandardSourceProvider_NoJobsRun_GoldFlow(int? federatedInstanceArtifactId)
        {
            // Arrange
            var model = new IntegrationPointDto()
            {
                ArtifactId = 123,
                SourceProvider = 9830,
                DestinationConfiguration = JsonConvert.SerializeObject(new ImportSettings() { FederatedInstanceArtifactId = federatedInstanceArtifactId }),
                SecuredConfiguration = _CREDENTIALS
            };

            _integrationPointService.SaveIntegrationPoint(Arg.Any<IntegrationPointDto>()).Returns(model.ArtifactId);

            string url = "http://lolol.com";
            _relativityUrlHelper.GetRelativityViewUrl(
                    _WORKSPACE_ID,
                    model.ArtifactId,
                    ObjectTypes.IntegrationPoint)
                .Returns(url);

            // Act
            HttpResponseMessage response = _sut.Update(_WORKSPACE_ID, model.ToWebModel(CamelCaseSerializer));

            // Assert
            Assert.IsNotNull(response, "Response should not be null");
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "HttpStatusCode should be OK");
            var result = response.Content.ReadAsStringAsync().Result;
            Assert.AreEqual(JsonConvert.SerializeObject(new { returnURL = url }), result, "The HttpContent should be as expected");

            _integrationPointService.Received(1).SaveIntegrationPoint(Arg.Any<IntegrationPointDto>());
            _relativityUrlHelper
                .Received(1)
                .GetRelativityViewUrl(
                    _WORKSPACE_ID,
                    model.ArtifactId,
                    ObjectTypes.IntegrationPoint);
        }

        [TestCase(null)]
        [TestCase(1000)]
        public void UpdateIntegrationPointThrowsError_ReturnFailedResponse(int? federatedInstanceArtifactId)
        {
            var model = new IntegrationPointDto()
            {
                DestinationConfiguration = JsonConvert.SerializeObject(new ImportSettings() { FederatedInstanceArtifactId = federatedInstanceArtifactId }),
                SecuredConfiguration = _CREDENTIALS
            };
            var validationResult = new ValidationResult(false, "That's a damn shame.");
            Exception expectException = new IntegrationPointValidationException(validationResult);
            _integrationPointService.SaveIntegrationPoint(Arg.Any<IntegrationPointDto>()).Throws(expectException);

            // Act
            HttpResponseMessage response = _sut.Update(_WORKSPACE_ID, model.ToWebModel(CamelCaseSerializer));

            Assert.IsNotNull(response);
            string actual = response.Content.ReadAsStringAsync().Result;
            _relativityUrlHelper.DidNotReceive().GetRelativityViewUrl(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<String>());
            ValidationResultDTO actualResult =
                JsonConvert.DeserializeObject<ValidationResultDTO>(actual);

            Assert.AreEqual(validationResult.MessageTexts.First(), actualResult.Errors.Single().Message);
            Assert.AreEqual(HttpStatusCode.NotAcceptable, response.StatusCode);
        }

        private async Task<IntegrationPointDto> GetIntegrationPointModelFromHttpResponse(HttpResponseMessage httpResponse)
        {
            string serializedModel = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonConvert.DeserializeObject<IntegrationPointDto>(serializedModel);
        }

        [Test]
        public void Update_ShouldLogMappingInfo()
        {
            // Arrange
            var model = new IntegrationPointDto()
            {
                ArtifactId = 123,
                SourceProvider = 9830,
                DestinationConfiguration = "",
                SecuredConfiguration = _CREDENTIALS
            };

            _integrationPointService.SaveIntegrationPoint(Arg.Is(model)).Returns(model.ArtifactId);

            string url = "http://lolol.com";
            _relativityUrlHelper.GetRelativityViewUrl(
                    _WORKSPACE_ID,
                    model.ArtifactId,
                    ObjectTypes.IntegrationPoint)
                .Returns(url);

            // Act
            HttpResponseMessage response = _sut.Update(_WORKSPACE_ID, model.ToWebModel(CamelCaseSerializer), true, true, true, IntegrationPointsAPIController.MappingType.SavedSearch);

            // Assert
            _loggerFake.Verify(x => x.LogInformation("Saved IntegrationPoint with following options: {options}",
                It.Is<object>(o => MappingInfoObjectIsCorrect(o))));
        }

        private bool MappingInfoObjectIsCorrect(object o)
        {
            dynamic dynamicObject = o;
            return dynamicObject.MappingHasWarnings == true
            && dynamicObject.ClearAndProceedSelected == true
            && dynamicObject.MappingType == MappingType.SavedSearch;
        }

    }
}
