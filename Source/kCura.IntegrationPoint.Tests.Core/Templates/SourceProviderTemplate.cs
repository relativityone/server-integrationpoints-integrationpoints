using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Castle.MicroKernel.Registration;
using kCura.Apps.Common.Config;
using kCura.Apps.Common.Data;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Authentication;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Factories.Implementations;
using kCura.IntegrationPoints.Core.Installers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Data.Installers;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Authentication;
using kCura.IntegrationPoints.Web;
using kCura.Relativity.Client;
using kCura.ScheduleQueue.Core;
using NUnit.Framework;
using Relativity.API;
using Relativity.Core;
using Relativity.Core.Authentication;
using Relativity.Core.Service;
using Relativity.Services.Agent;
using Relativity.Services.ResourceServer;
using Relativity.Toggles;
using Relativity.Toggles.Providers;

namespace kCura.IntegrationPoint.Tests.Core.Templates
{
	[TestFixture]
	public abstract class SourceProviderTemplate : IntegrationTestBase
	{
		protected bool CreateAgent { get; set; } = true;

		private readonly string _workspaceName;
		private readonly string _workspaceTemplate;
		protected int WorkspaceArtifactId { get; private set; }
		protected int AgentArtifactId { get; private set; }
		private bool _deleteAgentInTeardown = true;
		protected DestinationProvider DestinationProvider;
		protected IEnumerable<SourceProvider> SourceProviders;
		protected ICoreContext CoreContext;
		protected ICaseServiceContext CaseContext;
		protected RelativityApplicationManager RelativityApplicationManager;

		protected SourceProviderTemplate(string workspaceName,
			string workspaceTemplate = WorkspaceTemplates.NEW_CASE_TEMPLATE)
		{
			_workspaceName = workspaceName;
			_workspaceTemplate = workspaceTemplate;
			CoreContext = GetBaseServiceContext(-1);
			RelativityApplicationManager = new RelativityApplicationManager(CoreContext, Helper);
		}

		public override void SuiteSetup()
		{
			base.SuiteSetup();
			try
			{
				Manager.Settings.Factory = new HelperConfigSqlServiceFactory(Helper);
				WorkspaceArtifactId = Workspace.CreateWorkspace(_workspaceName, _workspaceTemplate);
				
				Install();

				Task.Run(async () => await SetupAsync()).Wait();

				CaseContext = Container.Resolve<ICaseServiceContext>();
				SourceProviders = CaseContext.RsapiService.SourceProviderLibrary.ReadAll(Guid.Parse(SourceProviderFieldGuids.Name), Guid.Parse(SourceProviderFieldGuids.Identifier));
				DestinationProvider = CaseContext.RsapiService.DestinationProviderLibrary.ReadAll(new Guid(DestinationProviderFieldGuids.Identifier))
					.First(x => x.Identifier == kCura.IntegrationPoints.Core.Constants.IntegrationPoints.RELATIVITY_DESTINATION_PROVIDER_GUID);
			}
			catch (Exception setupException)
			{
				try
				{
					SuiteTeardown();
				}
				catch (Exception teardownException)
				{
					Exception[] exceptions = new[] { setupException, teardownException };
					throw new AggregateException(exceptions);
				}
				throw;
			}
		}

		public override void SuiteTeardown()
		{
			Workspace.DeleteWorkspace(WorkspaceArtifactId);
			if (_deleteAgentInTeardown)
			{
				Agent.DeleteAgent(AgentArtifactId);
			}
			base.SuiteTeardown();
		}

		public static class WorkspaceTemplates
		{
			public const string NEW_CASE_TEMPLATE = "New Case Template";
		}

		protected virtual void Install()
		{
			Container.Register(Component.For<IHelper>().UsingFactoryMethod(k => Helper, managedExternally: true));
			Container.Register(Component.For<IRsapiClientFactory>().ImplementedBy<RsapiClientFactory>().LifestyleTransient());
			Container.Register(Component.For<IServiceContextHelper>()
				.UsingFactoryMethod(k =>
				{
					IHelper helper = k.Resolve<IHelper>();
					var rsapiClientFactory = k.Resolve<IRsapiClientFactory>();
					return new TestServiceContextHelper(helper, WorkspaceArtifactId, rsapiClientFactory);
				}));
			Container.Register(
				Component.For<IWorkspaceDBContext>()
					.ImplementedBy<WorkspaceContext>()
					.UsingFactoryMethod(k => new WorkspaceContext(k.Resolve<IHelper>().GetDBContext(WorkspaceArtifactId)))
					.LifeStyle.Transient);
			Container.Register(
				Component.For<IRSAPIClient>()
				.UsingFactoryMethod(k =>
				{
					IRSAPIClient client = Rsapi.CreateRsapiClient();
					client.APIOptions.WorkspaceID = WorkspaceArtifactId;
					return client;
				})
				.LifeStyle.Transient);

			Container.Register(Component.For<IWorkspaceService>().ImplementedBy<ControllerCustomPageService>().LifestyleTransient());
			Container.Register(Component.For<IWorkspaceService>().ImplementedBy<WebAPICustomPageService>().LifestyleTransient());
			Container.Register(Component.For<WebClientFactory>().ImplementedBy<WebClientFactory>().LifestyleTransient());
			Container.Register(Component.For<IRSAPIService>().Instance(new RSAPIService(Container.Resolve<IHelper>(), WorkspaceArtifactId)).LifestyleTransient());
			Container.Register(Component.For<IExporterFactory>().ImplementedBy<ExporterFactory>());
			Container.Register(Component.For<IAuthTokenGenerator>().ImplementedBy<ClaimsTokenGenerator>().LifestyleTransient());
			Container.Register(Component.For<IToggleProvider>().Instance(new SqlServerToggleProvider(
				() =>
				{
					SqlConnection connection = Container.Resolve<IHelper>().GetDBContext(-1).GetConnection(true);

					return connection;
				},
				async () =>
				{
					Task<SqlConnection> task = Task.Run(() =>
					{
						SqlConnection connection = Container.Resolve<IHelper>().GetDBContext(-1).GetConnection(true);
						return connection;
					});

					return await task;
				})).LifestyleTransient());

#pragma warning disable 618
			var dependencies = new IWindsorInstaller[] { new QueryInstallers(), new KeywordInstaller(), new ServicesInstaller(), new ValidationInstaller() };
#pragma warning restore 618

			foreach (IWindsorInstaller dependency in dependencies)
			{
				dependency.Install(Container, ConfigurationStore);
			}
		}

		#region Helper methods

		protected IntegrationPointModel CreateOrUpdateIntegrationPoint(IntegrationPointModel model)
		{
			IIntegrationPointService service = Container.Resolve<IIntegrationPointService>();

			int integrationPointArtifactId = service.SaveIntegration(model);

			IntegrationPoints.Data.IntegrationPoint rdo = service.GetRdo(integrationPointArtifactId);
			IntegrationPointModel newModel = IntegrationPointModel.FromIntegrationPoint(rdo);
			return newModel;
		}
		protected IntegrationPointProfileModel CreateOrUpdateIntegrationPointProfile(IntegrationPointProfileModel model)
		{
			IIntegrationPointProfileService service = Container.Resolve<IIntegrationPointProfileService>();

			int integrationPointArtifactId = service.SaveIntegration(model);

			IntegrationPoints.Data.IntegrationPointProfile rdo = service.GetRdo(integrationPointArtifactId);
			IntegrationPointProfileModel newModel = IntegrationPointProfileModel.FromIntegrationPointProfile(rdo);
			return newModel;
		}

		protected IDictionary<string, Tuple<string, string>> GetAuditDetailsFieldValues(Audit audit, HashSet<string> fieldNames)
		{
			var auditHelper = new AuditHelper(Helper);
			IDictionary<string, Tuple<string, string>> fieldValues = auditHelper.GetAuditDetailFieldUpdates(audit, fieldNames);

			return fieldValues;
		}

		protected IList<Audit> GetLastAuditsForIntegrationPoint(string integrationPointName, int take)
		{
			var auditHelper = new AuditHelper(Helper);

			IList<Audit> audits = auditHelper.RetrieveLastAuditsForArtifact(
				WorkspaceArtifactId,
				IntegrationPoints.Core.Constants.IntegrationPoints.INTEGRATION_POINT_OBJECT_TYPE_NAME,
				integrationPointName,
				take);

			return audits;
		}

		protected IntegrationPointModel RefreshIntegrationModel(IntegrationPointModel model)
		{
			ICaseServiceContext caseServiceContext = Container.Resolve<ICaseServiceContext>();

			var ip = caseServiceContext.RsapiService.IntegrationPointLibrary.Read(model.ArtifactID);
			return IntegrationPointModel.FromIntegrationPoint(ip);
		}

		protected void AssignJobToAgent(int agentId, long jobId)
		{
			string query = $"Update [{GlobalConst.SCHEDULE_AGENT_QUEUE_TABLE_NAME}] SET [LockedByAgentID] = @agentId,  [NextRunTime] = GETUTCDATE() - 1 Where JobId = @JobId";

			SqlParameter agentIdParam = new SqlParameter("@agentId", SqlDbType.Int) { Value = agentId };
			SqlParameter jobIdParam = new SqlParameter("@JobId", SqlDbType.BigInt) { Value = jobId };

			Helper.GetDBContext(-1).ExecuteNonQuerySQLStatement(query, new[] { agentIdParam, jobIdParam });


		}

		protected void ControlIntegrationPointAgents(bool enable)
		{
			AgentTypeRef agentType = Agent.GetAgentTypeByName("Integration Points Agent");

			string updateQuery = "UPDATE [Agent] SET [Enabled] = @enabledFlag, [Updated] = 1 WHERE [AgentTypeArtifactID] = @agentTypeArtifactId";
			string monitoringQuery = "SELECT Count(*) FROM [Agent] WHERE [AgentTypeArtifactID] = @agentTypeArtifactId AND [Updated] = 1";

			var enabledFlag = new SqlParameter("@enabledFlag", SqlDbType.Bit) {Value = enable};
			var agentTypeArtifactId = new SqlParameter("@agentTypeArtifactId", SqlDbType.Int) {Value = agentType.ArtifactID};

			IDBContext dbContext = Helper.GetDBContext(-1);

			Console.WriteLine($"Updating Integration Point agent state. Setting Enabled = '{enable}'");

			dbContext.ExecuteNonQuerySQLStatement(updateQuery, new[] { enabledFlag, agentTypeArtifactId });

			int attempts = 0;
			while (dbContext.ExecuteSqlStatementAsScalar<int>(monitoringQuery, agentTypeArtifactId) > 0)
			{
				if (attempts == 5)
				{
					throw new Exception("[INTEGRATION TESTS] Could not change state of Integration Point agent.");
				}
				attempts++;
				Thread.Sleep(2000);
				Console.WriteLine("Waiting for agent to update it's state...");
			}
			
			Console.WriteLine("Agent state updated (Update flag = 0 again).");
		}

		protected JobHistory CreateJobHistoryOnIntegrationPoint(int integrationPointArtifactId, Guid batchInstance, Relativity.Client.DTOs.Choice jobTypeChoice, Relativity.Client.DTOs.Choice jobStatusChoice = null, bool jobEnded = false)
		{
			IJobHistoryService jobHistoryService = Container.Resolve<IJobHistoryService>();
			IntegrationPoints.Data.IntegrationPoint integrationPoint = CaseContext.RsapiService.IntegrationPointLibrary.Read(integrationPointArtifactId);
			JobHistory jobHistory = jobHistoryService.CreateRdo(integrationPoint, batchInstance, jobTypeChoice, DateTime.Now);

			if (jobEnded)
			{
				jobHistory.EndTimeUTC = DateTime.Now;
			}

			if (jobStatusChoice != null)
			{
				jobHistory.JobStatus = jobStatusChoice;
			}

			if (jobEnded || jobStatusChoice != null)
			{
				jobHistoryService.UpdateRdo(jobHistory);
			}

			return jobHistory;
		}

		protected List<int> CreateJobHistoryError(int jobHistoryArtifactId, Relativity.Client.DTOs.Choice errorStatus, Relativity.Client.DTOs.Choice type)
		{
			List<JobHistoryError> jobHistoryErrors = new List<JobHistoryError>();
			JobHistoryError jobHistoryError = new JobHistoryError
			{
				ParentArtifactId = jobHistoryArtifactId,
				JobHistory = jobHistoryArtifactId,
				Name = Guid.NewGuid().ToString(),
				SourceUniqueID = type == ErrorTypeChoices.JobHistoryErrorItem ? Guid.NewGuid().ToString() : null,
				ErrorType = type,
				ErrorStatus = errorStatus,
				Error = "Inserted Error for testing.",
				StackTrace = "Error created from JobHistoryErrorsBatchingTests",
				TimestampUTC = DateTime.Now,
			};

			jobHistoryErrors.Add(jobHistoryError);

			List<int> jobHistoryErrorArtifactIds = CaseContext.RsapiService.JobHistoryErrorLibrary.Create(jobHistoryErrors);
			return jobHistoryErrorArtifactIds;
		}

		protected int GetLastScheduledJobId(int workspaceArtifactTypeId, int ripId)
		{
			const string query =
				"Select Top 1 JobId From [eddsdbo].[ScheduleAgentQueue_08C0CE2D-8191-4E8F-B037-899CEAEE493D]" +
				" Where [WorkspaceID] = @WorkspaceId AND [RelatedObjectArtifactID] = @RipId AND [ScheduleRuleType] IS NOT NULL Order By JobId DESC";

			SqlParameter workspaceId = new SqlParameter("@WorkspaceId", SqlDbType.Int) { Value = workspaceArtifactTypeId };
			SqlParameter integrationPointId = new SqlParameter("@RipId", SqlDbType.Int) { Value = ripId };

			return Helper.GetDBContext(-1).ExecuteSqlStatementAsScalar<int>(query, workspaceId, integrationPointId);
		}

		protected Job GetNextJobInScheduleQueue(int[] resourcePool, int integrationPointId)
		{
			IJobService jobServiceManager = Container.Resolve<IJobService>();

			List<Job> pickedUpJobs = new List<Job>();
			try
			{
				Job job;
				do
				{
					job = jobServiceManager.GetNextQueueJob(resourcePool, jobServiceManager.AgentTypeInformation.AgentTypeID);

					if (job != null)
					{
						// pick up job
						if (job.RelatedObjectArtifactID == integrationPointId)
						{
							return job;
						}
						else
						{
							pickedUpJobs.Add(job);
						}
					}
				} while (job != null);
			}
			finally
			{
				foreach (var pickedUpJob in pickedUpJobs)
				{
					jobServiceManager.UnlockJobs(pickedUpJob.AgentTypeID);
				}
			}
			throw new Exception("Unable to find the job. Please check the integration point agent and make sure that it is turned off.");
		}

		protected async Task SetupAsync()
		{
			await AddAgentServerToResourcePool();
			
			await Task.Run(() =>
			{
				if (SharedVariables.UseIpRapFile())
				{
					RelativityApplicationManager.ImportApplicationToLibrary();
				}

				RelativityApplicationManager.InstallApplicationFromLibrary(WorkspaceArtifactId);
				RelativityApplicationManager.DeployIntegrationPointsCustomPage();
			});
			
			if (CreateAgent)
			{
				Result agentCreatedResult = await Task.Run(() => Agent.CreateIntegrationPointAgent());
				AgentArtifactId = agentCreatedResult.ArtifactID;
				_deleteAgentInTeardown = agentCreatedResult.Success;
			}
		}

		private async Task AddAgentServerToResourcePool()
		{
			ResourceServer agentServer = await ResourceServerHelper.GetAgentServer(CoreContext);
			await ResourcePoolHelper.AddAgentServerToResourcePool(agentServer, "Default");
		}

		public static ICoreContext GetBaseServiceContext(int workspaceId)
		{
			try
			{
			    var loginManager = new LoginManager();
				Identity identity = loginManager.GetLoginIdentity(9);
				return new ServiceContext(identity, "<auditElement><RequestOrigination><IP /><Page /></RequestOrigination></auditElement>", workspaceId);
            }
			catch (Exception exception)
			{
				throw new Exception("Unable to initialize the user context.", exception);
			}
		}

		#endregion Helper methods
	}
}