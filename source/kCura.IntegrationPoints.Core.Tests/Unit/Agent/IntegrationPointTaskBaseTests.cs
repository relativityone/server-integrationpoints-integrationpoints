using System;
using System.Collections.Generic;
using System.Data;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Contracts.Provider;
using kCura.IntegrationPoints.Core.Agent;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain;
using kCura.ScheduleQueue.Core;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Tests.Unit.Agent
{
	[TestFixture]
	public class IntegrationPointTaskBaseTests
	{
		private TestClass _testInstance;

		protected ICaseServiceContext _caseServiceContext;
		protected IHelper _helper;
		protected IDataProviderFactory _dataProviderFactory;
		protected kCura.Apps.Common.Utils.Serializers.ISerializer _serializer;
		protected IJobHistoryService _jobHistoryService;
		protected JobHistoryErrorService _jobHistoryErrorService;
		protected ISynchronizerFactory _appDomainRdoSynchronizerFactoryFactory;
		protected IJobManager _jobManager;
		protected IManagerFactory _managerFactory;
		protected IContextContainerFactory _contextContainerFactory;
		protected IJobService _jobService;

		protected IContextContainer _contextContainer;

		[SetUp]
		public void SetUp()
		{
			_contextContainer = Substitute.For<IContextContainer>();
			_contextContainerFactory = Substitute.For<IContextContainerFactory>();
			_helper = Substitute.For<IHelper>();
			_jobService = Substitute.For<IJobService>();
			_managerFactory = Substitute.For<IManagerFactory>();
			_serializer = Substitute.For<kCura.Apps.Common.Utils.Serializers.ISerializer>();

			// Stubs
			_contextContainerFactory.CreateContextContainer(Arg.Is(_helper)).Returns(_contextContainer);

			_testInstance = new TestClass(_caseServiceContext,
				_helper,
				_dataProviderFactory,
				_serializer,
				_appDomainRdoSynchronizerFactoryFactory,
				_jobHistoryService,
				_jobHistoryErrorService,
				_jobManager,
				_managerFactory,
				_contextContainerFactory,
				_jobService);
		}

		[TestCase("")]
		[TestCase(" ")]
		[TestCase(";")]
		[TestCase(" ;")]
		[TestCase("; ")]
		[TestCase(" ; ")]
		[TestCase(";;")]
		[TestCase(" ; ; ")]
		public void GetRecipientEmails_NoEmailTests(string testEmail)
		{
			// ARRANGE
			var integrationPoint = new Data.IntegrationPoint
			{
				EmailNotificationRecipients = testEmail,
			};

			_testInstance.SetIntegrationPoint(integrationPoint);

			// ACT
			List<string> resultEmails = _testInstance.GetRecipientEmails();

			// ASSERT
			Assert.IsNotNull(resultEmails);
			Assert.AreEqual(0, resultEmails.Count);

		}

		[Test]
		public void GetRecipientEmails_OneEmailTest()
		{
			// ARRANGE
			string email1 = "email1@kcura.com";

			var integrationPoint = new Data.IntegrationPoint
			{
				EmailNotificationRecipients = email1,
			};

			_testInstance.SetIntegrationPoint(integrationPoint);

			// ACT
			List<string> resultEmails = _testInstance.GetRecipientEmails();

			// ASSERT
			Assert.IsNotNull(resultEmails);
			Assert.AreEqual(1, resultEmails.Count);
			Assert.AreEqual(email1, resultEmails[0]);
		}

		[Test]
		public void GetRecipientEmails_TwoEmailTest()
		{
			// ARRANGE
			string email1 = "email1@kcura.com";
			string email2 = "email2@kcura.com";

			var integrationPoint = new Data.IntegrationPoint
			{ 
				EmailNotificationRecipients = $"  {email1} ;   {email2}   ",
			};

			_testInstance.SetIntegrationPoint(integrationPoint);

			// ACT
			List<string> resultEmails = _testInstance.GetRecipientEmails();

			// ASSERT
			Assert.IsNotNull(resultEmails);
			Assert.AreEqual(2, resultEmails.Count);
			Assert.AreEqual(email1, resultEmails[0]);
			Assert.AreEqual(email2, resultEmails[1]);
		}

		[Test]
		public void GetRecipientEmails_MultipleEmailTest()
		{
			// ARRANGE
			string email1 = "email1@kcura.com";
			string email2 = "email2@kcura.com";
			string email3 = "email3@kcura.com";

			var integrationPoint = new Data.IntegrationPoint
			{
				EmailNotificationRecipients = $"  {email1} ;  ;;;  ;;;;  {email2}   ;  {email3} ;;;",
			};

			_testInstance.SetIntegrationPoint(integrationPoint);

			// ACT
			List<string> resultEmails = _testInstance.GetRecipientEmails();

			// ASSERT
			Assert.IsNotNull(resultEmails);
			Assert.AreEqual(3, resultEmails.Count);
			Assert.AreEqual(email1, resultEmails[0]);
			Assert.AreEqual(email2, resultEmails[1]);
			Assert.AreEqual(email3, resultEmails[2]);
		}

		[Test]
		public void ThrowIfStopRequested_ExceptsWhenStopped()
		{
			// ARRANGE
			const string jobDetailsText = "SERIALIZED";
			const long jobIdValue = 12321;
			IJobStopManager jobStopManager = Substitute.For<IJobStopManager>();

			var taskParameters = new TaskParameters()
			{
				BatchInstance = Guid.NewGuid(),
			};

			Job job = JobHelper.GetJob(jobIdValue, null, null, 0, 0, 0, 0, TaskType.SyncWorker, DateTime.Now, null,
				jobDetailsText, 0, DateTime.Now, 0, String.Empty, String.Empty);

			_serializer.Deserialize<TaskParameters>(Arg.Is<string>(x => x.Equals(jobDetailsText))).Returns(taskParameters);

			_managerFactory.CreateJobStopManager(Arg.Is(_contextContainer), Arg.Is(_jobService), Arg.Is(_jobHistoryService), Arg.Is(taskParameters.BatchInstance), Arg.Is(Convert.ToInt32(jobIdValue)))
				.Returns(jobStopManager);

			jobStopManager.When(x => x.ThrowIfStopRequested()).Throw(new OperationCanceledException());

			// ACT
			Assert.Throws<OperationCanceledException>(() => _testInstance.ThrowIfStopRequested(job));

			// ASSERT
			_serializer.Received(1).Deserialize<TaskParameters>(Arg.Is<string>(x => x.Equals(jobDetailsText)));
			_managerFactory.Received(1).CreateJobStopManager(Arg.Is(_contextContainer), Arg.Is(_jobService), Arg.Is(_jobHistoryService), Arg.Is(taskParameters.BatchInstance), Arg.Is(Convert.ToInt32(jobIdValue)));
			jobStopManager.Received(1).ThrowIfStopRequested();
		}

		[Test]
		public void ThrowIfStopRequested_DoesNotExceptWhenNotStopped()
		{
			// ARRANGE
			const string jobDetailsText = "SERIALIZED";
			const long jobIdValue = 12321;
			IJobStopManager jobStopManager = Substitute.For<IJobStopManager>();

			var taskParameters = new TaskParameters()
			{
				BatchInstance = Guid.NewGuid(),
			};

			Job job = JobHelper.GetJob(jobIdValue, null, null, 0, 0, 0, 0, TaskType.SyncWorker, DateTime.Now, null,
				jobDetailsText, 0, DateTime.Now, 0, String.Empty, String.Empty);

			_serializer.Deserialize<TaskParameters>(Arg.Is<string>(x => x.Equals(jobDetailsText))).Returns(taskParameters);

			_managerFactory.CreateJobStopManager(Arg.Is(_contextContainer), Arg.Is(_jobService), Arg.Is(_jobHistoryService),
				Arg.Is(taskParameters.BatchInstance), Arg.Is(Convert.ToInt32(jobIdValue)))
				.Returns(jobStopManager);

			jobStopManager.When(x => x.ThrowIfStopRequested()).Throw(new OperationCanceledException());

			// ACT
			Assert.Throws<OperationCanceledException>(() => _testInstance.ThrowIfStopRequested(job));

			// ASSERT
			_serializer.Received(1).Deserialize<TaskParameters>(Arg.Is<string>(x => x.Equals(jobDetailsText)));
			_managerFactory.Received(1).CreateJobStopManager(Arg.Is(_contextContainer), Arg.Is(_jobService), Arg.Is(_jobHistoryService), Arg.Is(taskParameters.BatchInstance), Arg.Is(Convert.ToInt32(jobIdValue)));
			jobStopManager.Received(1).ThrowIfStopRequested();
		}

		[Test]
		public void GetSourceProvider_ThrowsWhenStopIsRequested()
		{
			// ARRANGE	
			IJobStopManager jobStopManager = Substitute.For<IJobStopManager>();
			var exception = new OperationCanceledException();

			jobStopManager.When(x => x.ThrowIfStopRequested()).Throw(exception);


			// ACT
//			Assert.Throws<OperationCanceledException>(() => { _testInstance.GetSourceProvider()})

			// ASSERT
		}

	}

	public class TestClass : IntegrationPointTaskBase
	{
		public TestClass(ICaseServiceContext caseServiceContext,
			IHelper helper,
			IDataProviderFactory dataProviderFactory,
			kCura.Apps.Common.Utils.Serializers.ISerializer serializer,
			ISynchronizerFactory appDomainRdoSynchronizerFactoryFactory,
			IJobHistoryService jobHistoryService,
			JobHistoryErrorService jobHistoryErrorService,
			IJobManager jobManager,
			IManagerFactory managerFactory,
			IContextContainerFactory contextContainerFactory,
			IJobService jobService)
			: base(caseServiceContext, helper, dataProviderFactory, serializer, appDomainRdoSynchronizerFactoryFactory,
				jobHistoryService, jobHistoryErrorService, jobManager, managerFactory, contextContainerFactory, jobService)
		{
		}

		public void SetIntegrationPoint(Data.IntegrationPoint ip)
		{
			base.IntegrationPoint = ip;
		}

		public List<string> GetRecipientEmails()
		{
			return base.GetRecipientEmails();
		}

		public IDataSourceProvider GetSourceProvider(SourceProvider sourceProviderRdo, Job job)
		{
			return base.GetSourceProvider(sourceProviderRdo, job);
		}

		public void ThrowIfStopRequested(Job job)
		{
			base.ThrowIfStopRequested(job);
		}
	}
}
