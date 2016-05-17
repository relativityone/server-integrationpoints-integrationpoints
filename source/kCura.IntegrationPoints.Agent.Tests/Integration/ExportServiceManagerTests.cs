using System;
using System.Collections.Generic;
using Castle.MicroKernel.Registration;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Agent.Tasks;
using kCura.IntegrationPoints.Contracts;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data.Contexts;
using kCura.IntegrationPoints.Data.Factories;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.ScheduleRules;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.ObjectQuery;

namespace kCura.IntegrationPoints.Agent.Tests.Integration
{
	[TestFixture]
	[Explicit]
	public class ExportServiceManagerTests : WorkspaceDependentTemplate
	{
		private ExportServiceManager _exportManager;
		private IIntegrationPointService _integrationPointService;
		private IJobService _jobServiceManager;

		public ExportServiceManagerTests() : base("ExportServiceManagerTests", null)
		{ }

		protected override void Install()
		{
			base.Install();
			Container.Register(Component.For<JobStatisticsService>().ImplementedBy<JobStatisticsService>().LifeStyle.Transient);
			Container.Register(Component.For<IOnBehalfOfUserClaimsPrincipalFactory>()
				.ImplementedBy<OnBehalfOfUserClaimsPrincipalFactory>()
				.LifestyleTransient());
		}

		[TestFixtureTearDown]
		public override void TearDown()
		{ }

		[SetUp]
		public void TestSetup()
		{
			ICaseServiceContext caseContext = Container.Resolve<ICaseServiceContext>();
			IContextContainerFactory contextContainerFactory = Container.Resolve<IContextContainerFactory>();
			ISynchronizerFactory synchronizerFactory = Container.Resolve<ISynchronizerFactory>();
			IExporterFactory exporterFactory = Container.Resolve<IExporterFactory>();
			IOnBehalfOfUserClaimsPrincipalFactory onBehalfOfUserClaimsPrincipalFactory = Container.Resolve<IOnBehalfOfUserClaimsPrincipalFactory>();
			ISourceWorkspaceManager sourceWorkspaceManager = Container.Resolve<ISourceWorkspaceManager>();
			ISourceJobManager sourceJobManager = Container.Resolve<ISourceJobManager>();
			ITempDocumentTableFactory tempTableFactory = Container.Resolve<ITempDocumentTableFactory>();
			IRepositoryFactory repositoryFactory = Container.Resolve<IRepositoryFactory>();
			IManagerFactory managerFactory = Container.Resolve<IManagerFactory>();
			ISerializer serializer = Container.Resolve<ISerializer>();
			IJobService jobService = Container.Resolve<IJobService>();
			IScheduleRuleFactory scheduleRuleFactory = new DefaultScheduleRuleFactory();
			IJobHistoryService jobHistoryService = Container.Resolve<IJobHistoryService>();
			JobHistoryErrorService jobHistoryErrorManager = Container.Resolve<JobHistoryErrorService>();
			JobStatisticsService jobStatisticsService = Container.Resolve<JobStatisticsService>();

			_exportManager = new ExportServiceManager(Helper,
				caseContext, contextContainerFactory,
				synchronizerFactory,
				exporterFactory,
				onBehalfOfUserClaimsPrincipalFactory,
				sourceWorkspaceManager,
				sourceJobManager,
				tempTableFactory,
				repositoryFactory,
				managerFactory,
				new List<IBatchStatus>(),
				serializer,
				jobService,
				scheduleRuleFactory,
				jobHistoryService,
				jobHistoryErrorManager,
				jobStatisticsService
				);

			_integrationPointService = Container.Resolve<IIntegrationPointService>();
			_jobServiceManager = Container.Resolve<IJobService>();
		}

		[Test]
		public void Test()
		{
			IntegrationModel model = new IntegrationModel()
			{
				SourceProvider = RelativityProvider.ArtifactId,
				Name = "ARRRRRRRGGGHHHHH",
				DestinationProvider = DestinationProvider.ArtifactId,
				SourceConfiguration = CreateDefaultSourceConfig(),
				Destination = CreateDefaultDestinationConfig(),
				Map = CreateDefaultFieldMap(),
				Scheduler = new Scheduler()
				{
					EnableScheduler = false
				},
				SelectedOverwrite = "Append Only",
			};
			var result = CreateOrUpdateIntegrationPoint(model);

			Helper.GetServicesManager().CreateProxy<IObjectQueryManager>(ExecutionIdentity.CurrentUser).Returns(Helper.CreateUserObjectQueryManager(), Helper.CreateUserObjectQueryManager());
			_integrationPointService.RunIntegrationPoint(SourceWorkspaceArtifactId, result.ArtifactID, 9);

			Relativity.Client.DTOs.Workspace workspace = Workspace.GetWorkspaceDto(SourceWorkspaceArtifactId);
			Job job = _jobServiceManager.GetNextQueueJob(new[] { workspace.ResourcePoolID.Value  }, _jobServiceManager.AgentTypeInformation.AgentTypeID);
			Assert.IsNotNull(job);

			_exportManager.Execute(job);
		}
	}
}