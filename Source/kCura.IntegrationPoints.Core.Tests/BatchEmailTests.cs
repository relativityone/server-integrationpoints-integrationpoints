using System;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.Keywords;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain;
using kCura.ScheduleQueue.Core;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Tests
{
	[TestFixture]
	public class BatchEmailTests : TestBase
	{
		private BatchEmail _testInstance;

		private ICaseServiceContext _caseServiceContext;
		private IHelper _helper;
		private IDataProviderFactory _dataProviderFactory = null;
		private Apps.Common.Utils.Serializers.ISerializer _serializer = null;
		private ISynchronizerFactory _appDomainRdoSynchronizerFactoryFactory = null;
		private IJobHistoryService _jobHistoryService = null;
		private JobHistoryErrorService _jobHistoryErrorService = null;
		private IJobManager _jobManager = null;
		private IJobStatusUpdater _jobStatusUpdater = null;
		private KeywordConverter _converter = null;
		private IManagerFactory _managerFactory;
		private IContextContainerFactory _contextContainerFactory;
		private IJobService _jobService;
		private IIntegrationPointRepository _integrationPointRepository;

		private IRSAPIService _rsapiService;
		private IRelativityObjectManager _objectManager;

		private const int _INTEGRATION_POINT_ID = 1337;

		[SetUp]
		public override void SetUp()
		{
			_helper = Substitute.For<IHelper>();
			_caseServiceContext = Substitute.For<ICaseServiceContext>();
			_rsapiService = Substitute.For<IRSAPIService>();
			_objectManager = Substitute.For<IRelativityObjectManager>();
			_managerFactory = Substitute.For<IManagerFactory>();
			_contextContainerFactory = Substitute.For<IContextContainerFactory>();
			_jobService = Substitute.For<IJobService>();
			_integrationPointRepository = Substitute.For<IIntegrationPointRepository>();

			_rsapiService.RelativityObjectManager.Returns(_objectManager);
			_caseServiceContext.RsapiService.Returns(_rsapiService);


			_testInstance = new BatchEmail(_caseServiceContext,
				_helper,
				_dataProviderFactory, 
				_serializer, 
				_appDomainRdoSynchronizerFactoryFactory, 
				_jobHistoryService, 
				_jobHistoryErrorService, 
				_jobManager,
				_jobStatusUpdater,
				_converter,
				_managerFactory,
				_contextContainerFactory,
				_jobService,
				_integrationPointRepository);
		}

		[Test]
		public void OnJobComplete_NoEmails_Test()
		{
			// ARRANGE
			Job job = GetTestJob();

			var integrationPoint = new Data.IntegrationPoint
			{
				EmailNotificationRecipients = string.Empty,
			};

			_integrationPointRepository.ReadAsync(_INTEGRATION_POINT_ID).Returns(integrationPoint);

			// ACT + ASSERT
			Assert.DoesNotThrow(()=> { _testInstance.OnJobComplete(job); }, "Sending of email logic should have been skipped");
		}

		public static object[] GenerateEmailSource = new object[]
		{
			new object[] { JobStatusChoices.JobHistoryCompletedWithErrors, Properties.JobStatusMessages.JOB_COMPLETED_WITH_ERRORS_SUBJECT, Properties.JobStatusMessages.JOB_COMPLETED_WITH_ERRORS_BODY },
			new object[] { JobStatusChoices.JobHistoryErrorJobFailed, Properties.JobStatusMessages.JOB_FAILED_SUBJECT, Properties.JobStatusMessages.JOB_FAILED_BODY },
			new object[] { JobStatusChoices.JobHistoryStopped, Properties.JobStatusMessages.JOB_STOPPED_SUBJECT, Properties.JobStatusMessages.JOB_STOPPED_BODY },
			new object[] { JobStatusChoices.JobHistoryCompleted, Properties.JobStatusMessages.JOB_COMPLETED_SUCCESS_SUBJECT, Properties.JobStatusMessages.JOB_COMPLETED_SUCCESS_BODY },
		};

		[TestCaseSource(nameof(GenerateEmailSource))]
		public void GenerateEmail(Relativity.Client.DTOs.Choice jobStatus, string expectedSubject, string expectedBody)
		{
			// ACT
			EmailMessage message = _testInstance.GenerateEmail(jobStatus);

			// ASSERT
			Assert.AreEqual(expectedSubject, message.Subject);
			Assert.AreEqual(expectedBody, message.MessageBody);
		}

		private Job GetTestJob()
		{
			return JobHelper.GetJob(1337, null, null, 1, 1, 111, _INTEGRATION_POINT_ID, TaskType.ExportManager, new DateTime(), null, "", 0, new DateTime(), 1, null, null);
		}
	}
}
