using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Castle.Core.Internal;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.Relativity.Client;
using NUnit.Framework;

namespace kCura.IntegrationPoint.Tests.Core.Templates
{
	[TestFixture]
	[Category("Integration Tests")]
	public class WorkspaceDependentTemplate : SingleWorkspaceTestTemplate
	{
		private readonly string _targetWorkspaceName;

		protected SourceProvider LdapProvider;
		protected SourceProvider RelativityProvider;
		protected DestinationProvider DestinationProvider;
		protected ICaseServiceContext CaseContext;

		public int SourceWorkspaceArtifactId { get; protected set; }
		public int TargetWorkspaceArtifactId { get; protected set; }
		public int SavedSearchArtifactId { get; set; }
		public int AgentArtifactId { get; set; }

		public WorkspaceDependentTemplate(string sourceWorkspaceName, string targetWorkspaceName)
			: base(sourceWorkspaceName)
		{
			_targetWorkspaceName = targetWorkspaceName;
		}

		[TestFixtureSetUp]
		public virtual void SetUp()
		{
			SourceWorkspaceArtifactId = WorkspaceArtifactId;

			if (!_targetWorkspaceName.IsNullOrEmpty())
			{
				TargetWorkspaceArtifactId = Workspace.CreateWorkspace(_targetWorkspaceName, "New Case Template");
			}
			else
			{
				TargetWorkspaceArtifactId = SourceWorkspaceArtifactId;
			}

			Workspace.ImportLibraryApplicationToWorkspace(SourceWorkspaceArtifactId, new Guid(IntegrationPoints.Core.Constants.IntegrationPoints.APPLICATION_GUID_STRING));
			AgentArtifactId = Agent.CreateIntegrationPointAgent();

			SavedSearchArtifactId = SavedSearch.CreateSavedSearch(SourceWorkspaceArtifactId, "All documents");

			CaseContext = Container.Resolve<ICaseServiceContext>();
			IEnumerable<SourceProvider> providers = CaseContext.RsapiService.SourceProviderLibrary.ReadAll(Guid.Parse(SourceProviderFieldGuids.Name), Guid.Parse(SourceProviderFieldGuids.Identifier));
			RelativityProvider = providers.First(provider => provider.Name == "Relativity");
			LdapProvider = providers.First(provider => provider.Name == "LDAP");
			DestinationProvider = CaseContext.RsapiService.DestinationProviderLibrary.ReadAll().First();
		}

		[TestFixtureTearDown]
		public override void TearDown()
		{
			base.TearDown();
			Agent.DeleteAgent(AgentArtifactId);
		}

		protected IList<Audit> GetLastAuditsForIntegrationPoint(string integrationPointName, int take)
		{
			var auditHelper = new AuditHelper(Helper);

			IList<Audit> audits = auditHelper.RetrieveLastAuditsForArtifact(
				SourceWorkspaceArtifactId,
				IntegrationPoints.Core.Constants.IntegrationPoints.INTEGRATION_POINT_OBJECT_TYPE_NAME,
				integrationPointName,
				take);

			return audits;
		}

		protected IDictionary<string, Tuple<string, string>> GetAuditDetailsFieldValues(Audit audit, HashSet<string> fieldNames)
		{
			var auditHelper = new AuditHelper(Helper);
			IDictionary<string, Tuple<string, string>> fieldValues = auditHelper.GetAuditDetailFieldUpdates(audit, fieldNames);

			return fieldValues;
		}

		protected IntegrationModel CreateOrUpdateIntegrationPoint(IntegrationModel model)
		{
			IIntegrationPointService service = Container.Resolve<IIntegrationPointService>();

			int integrationPointArtifactId = service.SaveIntegration(model);

			IntegrationPoints.Data.IntegrationPoint rdo = service.GetRdo(integrationPointArtifactId);
			IntegrationModel newModel = new IntegrationModel(rdo);
			return newModel;
		}

		protected string CreateDefaultSourceConfig()
		{
			return $"{{\"SavedSearchArtifactId\":{SavedSearchArtifactId},\"SourceWorkspaceArtifactId\":\"{SourceWorkspaceArtifactId}\",\"TargetWorkspaceArtifactId\":{TargetWorkspaceArtifactId}}}";
		}

		protected string CreateDefaultDestinationConfig()
		{
			ImportSettings destinationConfig = new ImportSettings
			{
				ArtifactTypeId = 10,
				CaseArtifactId = SourceWorkspaceArtifactId,
				Provider = "Relativity",
				ImportOverwriteMode = ImportOverwriteModeEnum.AppendOnly,
				ImportNativeFile = false,
				ExtractedTextFieldContainsFilePath = false,
				FieldOverlayBehavior = "Use Field Settings",
				RelativityUsername = SharedVariables.RelativityUserName,
				RelativityPassword = SharedVariables.RelativityPassword
			};
			return Container.Resolve<ISerializer>().Serialize(destinationConfig);
		}

		protected IntegrationModel RefreshIntegrationModel(IntegrationModel model)
		{
			ICaseServiceContext caseServiceContext = Container.Resolve<ICaseServiceContext>();

			var ip = caseServiceContext.RsapiService.IntegrationPointLibrary.Read(model.ArtifactID);
			return new IntegrationModel(ip);
		}

		protected string CreateDefaultFieldMap()
		{
			FieldMap[] map = GetDefaultFieldMap();
			return Container.Resolve<ISerializer>().Serialize(map);
		}

		protected FieldMap[] GetDefaultFieldMap()
		{
			IRepositoryFactory repositoryFactory = Container.Resolve<IRepositoryFactory>();
			IFieldRepository sourceFieldRepository = repositoryFactory.GetFieldRepository(SourceWorkspaceArtifactId);
			IFieldRepository destinationFieldRepository = repositoryFactory.GetFieldRepository(TargetWorkspaceArtifactId);

			ArtifactDTO sourceDto = sourceFieldRepository.RetrieveTheIdentifierField((int)ArtifactType.Document);
			ArtifactDTO targetDto = destinationFieldRepository.RetrieveTheIdentifierField((int)ArtifactType.Document);

			FieldMap[] map = new[]
			{
				new FieldMap()
				{
					SourceField = new FieldEntry()
					{
						FieldIdentifier = sourceDto.ArtifactId.ToString(),
						DisplayName = sourceDto.Fields.First(field => field.Name == "Name").Value as string + " [Object Identifier]",
						IsIdentifier = true,
					},
					FieldMapType = FieldMapTypeEnum.Identifier,
					DestinationField = new FieldEntry()
					{
						FieldIdentifier = targetDto.ArtifactId.ToString(),
						DisplayName = targetDto.Fields.First(field => field.Name == "Name").Value as string + " [Object Identifier]",
						IsIdentifier = true,
					},
				}
			};
			return map;
		}

		protected void AssignJobToAgent(int agentId, long jobId)
		{
			string query = $" Update [{GlobalConst.SCHEDULE_AGENT_QUEUE_TABLE_NAME}] SET [LockedByAgentID] = @agentId,  [NextRunTime] = GETUTCDATE() - 1 Where JobId = @JobId";

			SqlParameter agentIdParam = new SqlParameter("@agentId", SqlDbType.BigInt) { Value = agentId };
			SqlParameter jobIdParam = new SqlParameter("@JobId", SqlDbType.Int) { Value = jobId };

			Helper.GetDBContext(-1).ExecuteNonQuerySQLStatement(query, new SqlParameter[] { agentIdParam, jobIdParam });
		}

		protected void ControlIntegrationPointAgents(bool enable)
		{
			string query = $@" Update A
  Set Enabled = @enabled
  From [Agent] A
	Inner Join [AgentType] AT
  ON A.AgentTypeArtifactID = AT.ArtifactID
  Where Guid = '{GlobalConst.RELATIVITY_INTEGRATION_POINTS_AGENT_GUID}'";

			SqlParameter toEnabled = new SqlParameter("@enabled", SqlDbType.Bit) { Value = enable };

			Helper.GetDBContext(-1).ExecuteNonQuerySQLStatement(query, new SqlParameter[] { toEnabled });
		}

		protected JobHistory CreateJobHistoryOnIntegrationPoint(int integrationPointArtifactId, Guid batchInstance)
		{
			IJobHistoryService jobHistoryService = Container.Resolve<IJobHistoryService>();
			IntegrationPoints.Data.IntegrationPoint integrationPoint = CaseContext.RsapiService.IntegrationPointLibrary.Read(integrationPointArtifactId);
			JobHistory jobHistory = jobHistoryService.CreateRdo(integrationPoint, batchInstance, JobTypeChoices.JobHistoryRunNow, DateTime.Now);
			jobHistory.EndTimeUTC = DateTime.Now;
			jobHistory.JobStatus = JobStatusChoices.JobHistoryCompletedWithErrors;
			jobHistoryService.UpdateRdo(jobHistory);
			return jobHistory;
		}

		protected List<int> CreateJobHistoryError(int jobHistoryArtifactId, Choice errorStatus, Choice type)
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
	}
}