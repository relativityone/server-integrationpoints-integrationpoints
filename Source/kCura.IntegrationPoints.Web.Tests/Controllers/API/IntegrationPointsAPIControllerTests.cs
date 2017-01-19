using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Factories.Implementations;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Services.Synchronizer;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.IntegrationPoints.Web.Controllers.API;
using Newtonsoft.Json;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Relativity.API;
using Relativity.Telemetry.Services.Metrics;
using Relativity.Toggles;

namespace kCura.IntegrationPoints.Web.Tests.Controllers.API
{
	[TestFixture]
	public class IntegrationPointsAPIControllerTests : TestBase
	{
		private IntegrationPointsAPIController _instance;
		private IServiceFactory _serviceFactory;
		private IIntegrationPointService _integrationPointService;
		private IRelativityUrlHelper _relativityUrlHelper;
		private IRdoSynchronizerProvider _rdoSynchronizerProvider;
		private ICPHelper _cpHelper;
		private ICaseServiceContext _caseServiceContext;
		private ISerializer _serializer;
		private IJobManager _jobManager;
		private IChoiceQuery _choiceQuery;
		private IHelperFactory _helperFactory;
		private IServicesMgr _svcMgr;
		private IContextContainerFactory _contextContainerFactory;
		private IJobHistoryService _jobHistoryService;
		private IManagerFactory _managerFactory;
		private IIntegrationPointProviderValidator _ipValidator;
		private IIntegrationPointPermissionValidator _permissionValidator;
		private IToggleProvider _toggleProvider;

		private const int _WORKSPACE_ID = 23432;

		[SetUp]
		public override void SetUp()
		{
			_relativityUrlHelper = Substitute.For<IRelativityUrlHelper>();
			_integrationPointService = Substitute.For<IIntegrationPointService>();
			_rdoSynchronizerProvider = Substitute.For<IRdoSynchronizerProvider>();
			_serviceFactory = Substitute.For<IServiceFactory>();
			_cpHelper = Substitute.For<ICPHelper>();
			_svcMgr = Substitute.For<IServicesMgr>();
			_caseServiceContext = Substitute.For<ICaseServiceContext>();
			_serializer = Substitute.For<ISerializer>();
			_jobManager = Substitute.For<IJobManager>();
			_choiceQuery = Substitute.For<IChoiceQuery>();
			_helperFactory = Substitute.For<IHelperFactory>();
			_contextContainerFactory = Substitute.For<IContextContainerFactory>();
			_jobHistoryService = Substitute.For<IJobHistoryService>();
			_managerFactory = Substitute.For<IManagerFactory>();
			_ipValidator = Substitute.For<IIntegrationPointProviderValidator>();
			_permissionValidator = Substitute.For<IIntegrationPointPermissionValidator>();
			_toggleProvider = Substitute.For<IToggleProvider>();

			_cpHelper.GetServicesManager().Returns(_svcMgr);
			_svcMgr.CreateProxy<IMetricsManager>(Arg.Any<ExecutionIdentity>()).Returns(Substitute.For<IMetricsManager>());

			_instance = new IntegrationPointsAPIController(_serviceFactory, _relativityUrlHelper, _rdoSynchronizerProvider, _cpHelper, _caseServiceContext, _contextContainerFactory,
				_serializer, _choiceQuery, _jobManager, _jobHistoryService, _managerFactory, _helperFactory, _ipValidator, _permissionValidator, _toggleProvider) {Request = new HttpRequestMessage()};

			_instance.Request.SetConfiguration(new HttpConfiguration());
		}

		[Test]
		public void Update_StandardSourceProvider_NoJobsRun_GoldFlow()
		{
			// Arrange
			var model = new IntegrationPointModel()
			{
				ArtifactID = 123,
				SourceProvider = 9830,
				Destination = JsonConvert.SerializeObject(new ImportSettings())
			};

			_serviceFactory.CreateIntegrationPointService(_cpHelper, _cpHelper, _caseServiceContext, _contextContainerFactory, _serializer, _choiceQuery, 
				_jobManager, _managerFactory, _ipValidator, _permissionValidator, _toggleProvider).Returns(_integrationPointService);

			_integrationPointService.SaveIntegration(Arg.Is(model)).Returns(model.ArtifactID);

			string url = "http://lolol.com";
			_relativityUrlHelper.GetRelativityViewUrl(
					_WORKSPACE_ID,
					model.ArtifactID,
					ObjectTypes.IntegrationPoint)
				.Returns(url);

			// Act
			HttpResponseMessage response = _instance.Update(_WORKSPACE_ID, model);

			// Assert
			Assert.IsNotNull(response, "Response should not be null");
			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "HttpStatusCode should be OK");
			Assert.AreEqual(JsonConvert.SerializeObject(new { returnURL = url }), response.Content.ReadAsStringAsync().Result, "The HttpContent should be as expected");

			_integrationPointService.Received(1).SaveIntegration(model);
			_relativityUrlHelper
				.Received(1)
				.GetRelativityViewUrl(
					_WORKSPACE_ID,
					model.ArtifactID,
					ObjectTypes.IntegrationPoint);
		}

		[Test]
		public void UpdateIntegrationPointThrowsError_ReturnFailedResponse()
		{
			var model = new IntegrationPointModel()
			{
				Destination = JsonConvert.SerializeObject(new ImportSettings())
			};
			var validationResult = new ValidationResult(false, "That's a damn shame.");
			Exception expectException = new IntegrationPointProviderValidationException(validationResult);
			_serviceFactory.CreateIntegrationPointService(_cpHelper, _cpHelper, _caseServiceContext, _contextContainerFactory, _serializer, _choiceQuery, 
				_jobManager, _managerFactory, _ipValidator, _permissionValidator, _toggleProvider).Returns(_integrationPointService);
			_integrationPointService.SaveIntegration(Arg.Any<IntegrationPointModel>()).Throws(expectException);

			// Act
			HttpResponseMessage response = _instance.Update(_WORKSPACE_ID, model);

			Assert.IsNotNull(response);
			String actual = response.Content.ReadAsStringAsync().Result;
			_relativityUrlHelper.DidNotReceive().GetRelativityViewUrl(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<String>());
			Assert.AreEqual($"\"{validationResult.Messages.First()}\"", actual);
			Assert.AreEqual(HttpStatusCode.NotAcceptable, response.StatusCode);
		}
	}
}