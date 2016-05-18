using System;
using System.Collections.Generic;
using System.Linq;
using Castle.MicroKernel.Registration;
using kCura.Apps.Common.Data;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Installers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Installers;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.Relativity.Client;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.ObjectQuery;

namespace kCura.IntegrationPoint.Tests.Core.Templates
{
	[TestFixture]
	[Category("Integration Tests")]
	[Explicit]
	public class WorkspaceDependentTemplate : IntegrationTestBase
	{
		private readonly string _sourceWorkspaceName;
		private readonly string _targetWorkspaceName;
		protected SourceProvider RelativityProvider;
		protected DestinationProvider DestinationProvider;
		protected ICaseServiceContext CaseContext;

		public int SourceWorkspaceArtifactId { get; private set; }
		public int TargetWorkspaceArtifactId { get; private set; }
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
			TargetWorkspaceArtifactId = SourceWorkspaceArtifactId;
			Workspace.ImportApplicationToWorkspace(SourceWorkspaceArtifactId, SharedVariables.RapFileLocation, true);
			SavedSearchArtifactId = SavedSearch.CreateSavedSearch(SourceWorkspaceArtifactId, "All documents");
			Install();

			CaseContext = Container.Resolve<ICaseServiceContext>();
			IEnumerable<SourceProvider> providers = CaseContext.RsapiService.SourceProviderLibrary.ReadAll(Guid.Parse(SourceProviderFieldGuids.Name),
							Guid.Parse(SourceProviderFieldGuids.Identifier));
			RelativityProvider = providers.First(provider => provider.Name == "Relativity");
			DestinationProvider = CaseContext.RsapiService.DestinationProviderLibrary.ReadAll().First();
		}

		protected void Install()
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
			Container.Register(Component.For<IPermissionRepository>().UsingFactoryMethod(k => Helper.PermissionManager));

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

		protected IntegrationModel CreateOrUpdateIntegrationPoint(IntegrationModel model)
		{
			Helper.PermissionManager.UserCanEditDocuments().Returns(true);
			Helper.PermissionManager.UserCanImport().Returns(true);
			Helper.PermissionManager.UserCanViewArtifact(Arg.Any<int>(), Arg.Any<int>()).Returns(true);

			IIntegrationPointService service = Container.Resolve<IIntegrationPointService>();

			int integrationPointAritfactId = service.SaveIntegration(model);

			var rdo = service.GetRdo(integrationPointAritfactId);
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
			IFieldRepository sourcefieldRepository = repositoryFactory.GetFieldRepository(SourceWorkspaceArtifactId);
			IFieldRepository destinationfieldRepository = repositoryFactory.GetFieldRepository(TargetWorkspaceArtifactId);

			Helper.GetServicesManager().CreateProxy<IObjectQueryManager>(ExecutionIdentity.CurrentUser).Returns(Helper.CreateUserObjectQueryManager(), Helper.CreateUserObjectQueryManager());
			ArtifactDTO sourceDto = sourcefieldRepository.RetrieveTheIdentifierField((int)ArtifactType.Document);
			ArtifactDTO targetDto = destinationfieldRepository.RetrieveTheIdentifierField((int)ArtifactType.Document);

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
	}
}