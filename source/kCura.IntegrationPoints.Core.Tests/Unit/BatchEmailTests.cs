using System;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.Keywords;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain;
using kCura.ScheduleQueue.Core;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Tests.Unit
{
	[TestFixture]
	public class BatchEmailTests
	{
		private BatchEmail _testInstance;

		private ICaseServiceContext _caseServiceContext;
		private IHelper _helper;
		private IDataProviderFactory _dataProviderFactory;
		private kCura.Apps.Common.Utils.Serializers.ISerializer _serializer;
		private ISynchronizerFactory _appDomainRdoSynchronizerFactoryFactory;
		private IJobHistoryService _jobHistoryService;
		private JobHistoryErrorService _jobHistoryErrorService;
		private IJobManager _jobManager;
		private IJobStatusUpdater _jobStatusUpdater;
		private KeywordConverter _converter;
		private IManagerFactory _managerFactory;

		private IRSAPIService _rsapiService;
		private IGenericLibrary<Data.IntegrationPoint> _integrationPointLibrary;

		private const int _INTEGRATION_POINT_ID = 1337;

		[SetUp]
		public void SetUp()
		{
			_caseServiceContext = Substitute.For<ICaseServiceContext>();
			_rsapiService = Substitute.For<IRSAPIService>();
			_integrationPointLibrary = Substitute.For<IGenericLibrary<Data.IntegrationPoint>>();
			_managerFactory = Substitute.For<IManagerFactory>();

			_rsapiService.IntegrationPointLibrary.Returns(_integrationPointLibrary);
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
				_managerFactory);
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

			_integrationPointLibrary.Read(_INTEGRATION_POINT_ID).Returns(integrationPoint);

			// ACT + ASSERT
			Assert.DoesNotThrow(()=> { _testInstance.OnJobComplete(job); }, "Sending of email logic should have been skipped");
		}

		private Job GetTestJob()
		{
			return JobHelper.GetJob(1337, null, null, 1, 1, 111, _INTEGRATION_POINT_ID, TaskType.ExportManager, new DateTime(), null, "", 0, new DateTime(), 1, null, null);
		}
	}
}
