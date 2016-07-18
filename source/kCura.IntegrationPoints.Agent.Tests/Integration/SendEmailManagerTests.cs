using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

using global::Relativity.API;

using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Agent.Tasks;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Data.Queries;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Data;

using Newtonsoft.Json;

namespace kCura.IntegrationPoints.Core.Tests.Integration.Services
{
	

	[TestFixture]
	[Category("Integration Tests")]
	public class SendEmailManagerTests : RelativityProviderTemplate
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



		public SendEmailManagerTests()
			: base("IntegrationPointSource", null)
		{
		}

		[SetUp]

		[TestFixtureSetUp]
		public void SuiteSetUp()
		{
			_repositoryFactory = Container.Resolve<IRepositoryFactory>();
			_jobService = Container.Resolve<IJobService>();
			_serializer = Container.Resolve<ISerializer>();
			_eddsServiceContext = Container.Resolve<IEddsServiceContext>();
			_workspaceDbContext = Container.Resolve<IWorkspaceDBContext>();
			_jobManager = this.Container.Resolve<IJobManager>();
			_queueContext = new QueueDBContext(Helper, GlobalConst.SCHEDULE_AGENT_QUEUE_TABLE_NAME);

			_jobResource = new JobResourceTracker(_repositoryFactory, _workspaceDbContext);
			_jobTracker = new JobTracker(_jobResource);
			_manager = new AgentJobManager(_eddsServiceContext, _jobService, _serializer, _jobTracker);

			_sendEmailManager = new SendEmailManager(this._serializer, this._jobManager);

			//agemta
		}

		/// <summary>
		/// 
		/// </summary>
		[Test]
		public void VerifyGetUnbatchedID()
		{
			//Arrange
			//IntegrationModel integrationModel = new IntegrationModel
			//{
			//	Destination = CreateDefaultDestinationConfig(),
			//	DestinationProvider = DestinationProvider.ArtifactId,
			//	SourceProvider = RelativityProvider.ArtifactId,
			//	SourceConfiguration = CreateDefaultSourceConfig(),
			//	LogErrors = true,
			//	Name = "SendEmailManager" + DateTime.Now,
			//	SelectedOverwrite = "Append Only",
			//	Scheduler = new Scheduler()
			//	{
			//		EnableScheduler = true
			//	},
			//	Map = CreateDefaultFieldMap(),
			//	NotificationEmails = "kwu@kcura.com;gthurman@kcura.com"
			//};

			//IntegrationModel integrationPoint = CreateOrUpdateIntegrationPoint(integrationModel);

			//int jobId = 1;
			//int rootJobId = 1;

			////string details = "{\"BatchInstance\":\"e324aeb8-71aa-4adb-a0cb-2174efcc3e01\",\"BatchParameters\":null}";
			//Job job = this._jobService.GetScheduledJob(SourceWorkspaceArtifactId, integrationPoint.ArtifactID,TaskType.ExportService.ToString());

			//Arrange
			EmailMessage emailMessage = new EmailMessage
			{
				Subject = "Send Email Manager Testing",
				MessageBody = "GeeeRizzle",
				Emails = new[] { "testing1234@kcura.com", "kwu@kcura.com" }
			};

			//Job parentJob = JobExtensions.CreateJob(SourceWorkspaceArtifactId, 1,1,1);
			//_jobManager.CreateJob(parentJob, emailMessage, TaskType.SendEmailManager);

			string jobDetails = "{\"Subject\":\"testing stuff\",\"MessageBody\":\"Hello, this is GeeeRizzle \",\"Emails\":[\"testing1234@kcura.com\",\"kwu@kcura.com\"]}";
			string scheduleRule = "Rule";
			
			int jobId = JobExtensions.Execute(
				this._queueContext,
				SourceWorkspaceArtifactId,
				1,
				"SendEmailManager",
				DateTime.Now,
				1,
				null,
				scheduleRule,
				jobDetails,
				1,
				777,
				10101,
				1,
				1);


			Job tempJob = this._jobManager.GetJob(SourceWorkspaceArtifactId, 1, "SendEmailManager");

			//Act
			IEnumerable<string> list = _sendEmailManager.GetUnbatchedIDs(tempJob);

			//Assert
			Assert.AreEqual(2, list.Count());
			Assert.IsTrue(list.Contains("testing1234@kcura.com"));
			Assert.IsTrue(list.Contains("kwu@kcura.com"));
		}



	}
}