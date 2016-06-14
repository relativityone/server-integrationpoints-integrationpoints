using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Castle.Core.Internal;
using Castle.MicroKernel.Registration;
using kCura.Apps.Common.Data;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.RDO;
using kCura.IntegrationPoints.Core.Installers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Installers;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using NUnit.Framework;
using Relativity.API;
using Relativity.Core;
using Relativity.Core.Service;
using Relativity.Services.ObjectQuery;

namespace kCura.IntegrationPoint.Tests.Core.Templates
{
	[TestFixture]
	[Category("Integration Tests")]
	public class WorkspaceDependentTemplate : IntegrationTestBase
	{
		private readonly string _sourceWorkspaceName;
		private readonly string _targetWorkspaceName;

		protected SourceProvider LdapProvider;
		protected SourceProvider RelativityProvider;
		protected DestinationProvider DestinationProvider;
		protected ICaseServiceContext CaseContext;

		public int SourceWorkspaceArtifactId { get; protected set; }
		public int TargetWorkspaceArtifactId { get; protected set; }
		public int SavedSearchArtifactId { get; set; }

		public WorkspaceDependentTemplate(string sourceWorkspaceName, string targetWorkspaceName)
		{
			_sourceWorkspaceName = sourceWorkspaceName;
			_targetWorkspaceName = targetWorkspaceName;
		}

		[TestFixtureSetUp]
		public virtual void SetUp()
		{
			Apps.Common.Config.Manager.Settings.Factory = new HelperConfigSqlServiceFactory(Helper);
			const string template = "New Case Template";
			SourceWorkspaceArtifactId = Workspace.CreateWorkspace(_sourceWorkspaceName, template);

			if (!_targetWorkspaceName.IsNullOrEmpty())
			{
				TargetWorkspaceArtifactId = Workspace.CreateWorkspace(_targetWorkspaceName, template);
			}
			else
			{
				TargetWorkspaceArtifactId = SourceWorkspaceArtifactId;
			}

			Workspace.ImportApplicationToWorkspace(SourceWorkspaceArtifactId, SharedVariables.RapFileLocation, true);
			SavedSearchArtifactId = SavedSearch.CreateSavedSearch(SourceWorkspaceArtifactId, "All documents");
			Install();

			CaseContext = Container.Resolve<ICaseServiceContext>();
			IEnumerable<SourceProvider> providers = CaseContext.RsapiService.SourceProviderLibrary.ReadAll(Guid.Parse(SourceProviderFieldGuids.Name), Guid.Parse(SourceProviderFieldGuids.Identifier));
			RelativityProvider = providers.First(provider => provider.Name == "Relativity");
			LdapProvider = providers.First(provider => provider.Name == "LDAP");
			DestinationProvider = CaseContext.RsapiService.DestinationProviderLibrary.ReadAll().First();
		}

		protected virtual void Install()
		{
			Container.Register(Component.For<IHelper>().UsingFactoryMethod(k => Helper, managedExternally: true));
			Container.Register(Component.For<IServiceContextHelper>()
				.UsingFactoryMethod(k =>
				{
					IHelper helper = k.Resolve<IHelper>();
					return new TestServiceContextHelper(helper, SourceWorkspaceArtifactId);
				}));
			Container.Register(Component.For<ICaseServiceContext>().ImplementedBy<CaseServiceContext>().LifestyleTransient());
			Container.Register(Component.For<IEddsServiceContext>().ImplementedBy<EddsServiceContext>().LifestyleTransient());
			Container.Register(
				Component.For<IWorkspaceDBContext>()
					.ImplementedBy<WorkspaceContext>()
					.UsingFactoryMethod(k => new WorkspaceContext(k.Resolve<IHelper>().GetDBContext(SourceWorkspaceArtifactId)))
					.LifeStyle.Transient);

			Container.Register(
				Component.For<IRSAPIClient>()
				.UsingFactoryMethod(k =>
				{
					IRSAPIClient client = Rsapi.CreateRsapiClient();
					client.APIOptions.WorkspaceID = SourceWorkspaceArtifactId;
					return client;
				})
				.LifeStyle.Transient);

			Container.Register(Component.For<IServicesMgr>().UsingFactoryMethod(k => Helper.GetServicesManager()));
			Container.Register(Component.For<IQueueRepository>().ImplementedBy<QueueRepository>().LifestyleTransient());

			var dependencies = new IWindsorInstaller[] { new QueryInstallers(), new KeywordInstaller(), new ServicesInstaller() };
			foreach (IWindsorInstaller dependency in dependencies)
			{
				dependency.Install(Container, ConfigurationStore);
			}
		}

		[TestFixtureTearDown]
		public virtual void TearDown()
		{
			Workspace.DeleteWorkspace(SourceWorkspaceArtifactId);
			Workspace.DeleteWorkspace(TargetWorkspaceArtifactId);
		}

		protected Audit GetLastForIntegrationPoint(string integrationPointName)
		{
			var auditHelper = new AuditHelper(Helper);

			Audit audit = auditHelper.RetrieveLastAuditForArtifact(SourceWorkspaceArtifactId, "IntegrationPoint", integrationPointName);

			return audit;
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
				FieldOverlayBehavior = "Use Field Settings"
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
			return Container.Resolve<ISerializer>().Serialize(map);
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
	}
}