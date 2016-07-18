using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Agent.Tasks;
using NUnit.Framework;
using System;

namespace kCura.IntegrationPoints.Core.Tests.Integration.Services
{
	using System.Collections.Generic;
	using System.Linq;

	using Castle.Windsor;

	using global::Relativity.API;

	using kCura.IntegrationPoint.Tests.Core;
	using kCura.IntegrationPoints.Agent.Tasks;
	using kCura.IntegrationPoints.Core.Contracts.Agent;
	using kCura.IntegrationPoints.Core.Factories;
	using kCura.IntegrationPoints.Core.Services.JobHistory;
	using kCura.IntegrationPoints.Data.Queries;
	using kCura.IntegrationPoints.Data.Repositories;
	using kCura.Relativity.Client;
	using kCura.ScheduleQueue.AgentBase;
	using kCura.ScheduleQueue.Core;
	using kCura.ScheduleQueue.Core.Data;
	using kCura.ScheduleQueue.Core.ScheduleRules;

	using Newtonsoft.Json;

	using NSubstitute;
	using NSubstitute.ExceptionExtensions;

	using IntegrationPoint = kCura.IntegrationPoints.Data.IntegrationPoint;

	[TestFixture]
	[Category("Integration Tests")]
	public class ITaskFactoryTests
	{
		private const int _ADMIN_USER_ID = 9;
		private IRepositoryFactory _repositoryFactory;
		private AgentJobManager _manager;
		private IEddsServiceContext _eddsServiceContext;
		private IJobService _jobService;
		private ISerializer _serializer;
		private JobTracker _jobTracker;
		private IWorkspaceDBContext _workspaceDbContext;
		private JobResourceTracker _jobResource;
		private IScratchTableRepository _scratchTableRepository;

		private IQueueDBContext _queueContext;

		private IJobManager _jobManager;

		private SendEmailManager _sendEmailManager;

		//private int artifactidID = Agent.CreateIntegrationPointAgent();



		//public ITaskFactoryTests()
		//	: base("IntegrationPointSource", null)
		//{
		//}

		//[SetUp]

		[TestFixtureSetUp]
		public void SuiteSetUp()
		{
			//_repositoryFactory = Container.Resolve<IRepositoryFactory>();
			//_jobService = Container.Resolve<IJobService>();
			//_serializer = Container.Resolve<ISerializer>();
			//_eddsServiceContext = Container.Resolve<IEddsServiceContext>();
			//_workspaceDbContext = Container.Resolve<IWorkspaceDBContext>();
			//_jobManager = this.Container.Resolve<IJobManager>();
			//_queueContext = new QueueDBContext(Helper, GlobalConst.SCHEDULE_AGENT_QUEUE_TABLE_NAME);

			//_jobResource = new JobResourceTracker(_repositoryFactory, _workspaceDbContext);
			//_jobTracker = new JobTracker(_jobResource);
			//_manager = new AgentJobManager(_eddsServiceContext, _jobService, _serializer, _jobTracker);

			//_sendEmailManager = new SendEmailManager(this._serializer, this._jobManager);

			//agemta
		}

		/// <summary>
		/// 
		/// </summary>
		//[Test]
		//public void VerifyUpdateOnBatchFailure()
		//{
		//	Job nullJob = null;
		//}

		[Test]
		public void UpdateJobHistory()
		{
			// arrange
			Job tempJob = JobExtensions.CreateJob();
			Exception excep = null;
			IAgentHelper helper = Substitute.For<IAgentHelper>();
			IntegrationPoint integrationPoint = new IntegrationPoint();
			TaskParameters paramerters = new TaskParameters();
			JobHistory jobHistory = new JobHistory();

			ScheduleQueueAgentBase agentBase = new TestAgentBase(Guid.NewGuid());
			IWindsorContainer container = Substitute.For<IWindsorContainer>();
			ICaseServiceContext caseServiceContext = Substitute.For<ICaseServiceContext>();
			ISerializer serializer = Substitute.For<ISerializer>();
			IJobHistoryService jobHistoryService = Substitute.For<IJobHistoryService>();

			caseServiceContext.RsapiService.IntegrationPointLibrary.Read(Arg.Any<int>()).Returns(integrationPoint);
			serializer.Deserialize<TaskParameters>(String.Empty).Returns(paramerters);
			jobHistoryService.GetRdo(paramerters.BatchInstance).Returns(jobHistory);

			container.Resolve<SyncManager>().Throws(new Exception("Gerron is a bad boy."));
			//container.Resolve<ISerializer>().Returns(this.Container.Resolve<ISerializer>());
			//container.Resolve<ICaseServiceContext>().Returns(caseServiceContext);
			//container.Resolve<IRSAPIClient>().Returns(this.Container.Resolve<IRSAPIClient>());
			//container.Resolve<IWorkspaceDBContext>().Returns(this.Container.Resolve<IWorkspaceDBContext>());
			//container.Resolve<IEddsServiceContext>().Returns(this.Container.Resolve<IEddsServiceContext>());
			//container.Resolve<IRepositoryFactory>().Returns(this.Container.Resolve<IRepositoryFactory>());
			//container.Resolve<IJobHistoryService>().Returns(this.Container.Resolve<IJobHistoryService>());
			//container.Resolve<IContextContainerFactory>().Returns(this.Container.Resolve<IContextContainerFactory>());

			TaskFactory taskFactory = new TaskFactory(helper, container);

			// act
			taskFactory.CreateTask(tempJob, agentBase);


			// assert
			caseServiceContext.RsapiService.JobHistoryErrorLibrary.Received(1).Create(Arg.Any<List<JobHistoryError>>());
			//ITask task = kCura.IntegrationPoints.Agent.Tasks.ITaskFactory.UpdateJobHistoryOnFailure(tempJob, excep);
		}

/*
		public class TestAgentHelper : IAgentHelper
		{
			private readonly IHelper _helper;

			public TestAgentHelper(IHelper helper)
			{
				this._helper = helper;
			}

			public void Dispose()
			{
				_helper.Dispose();
			}

			public IDBContext GetDBContext(int caseID)
			{
				return _helper.GetDBContext(caseID);
			}

			public IServicesMgr GetServicesManager()
			{
				return this._helper.GetServicesManager();
			}

			public IUrlHelper GetUrlHelper()
			{
				throw new NotImplementedException();
			}

			public ILogFactory GetLoggerFactory()
			{
				return this._helper.GetLoggerFactory();
			}

			public string ResourceDBPrepend()
			{
				return this._helper.ResourceDBPrepend();
			}

			public string ResourceDBPrepend(IDBContext context)
			{
				return this._helper.ResourceDBPrepend(context);
			}

			public string GetSchemalessResourceDataBasePrepend(IDBContext context)
			{
				return this._helper.GetSchemalessResourceDataBasePrepend(context);
			}

			public Guid GetGuid(int workspaceID, int artifactID)
			{
				return this._helper.GetGuid(workspaceID, artifactID);
			}

			public IAuthenticationMgr GetAuthenticationManager()
			{
				throw new NotImplementedException();
			}
		}
		*/
		public class TestAgentBase : ScheduleQueueAgentBase
		{
			public TestAgentBase(Guid agentGuid, IDBContext dbContext = null, IAgentService agentService = null, IJobService jobService = null, IScheduleRuleFactory scheduleRuleFactory = null)
				: base(agentGuid, dbContext, agentService, jobService, scheduleRuleFactory)
			{
			}

			public override string Name { get; }
		}

	}
}