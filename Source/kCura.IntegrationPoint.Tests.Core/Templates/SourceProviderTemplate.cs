using Castle.MicroKernel.Registration;
using kCura.Apps.Common.Config;
using kCura.Apps.Common.Data;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Factories.Implementations;
using kCura.IntegrationPoints.Core.Installers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Installers;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Authentication;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Castle.MicroKernel.Resolvers;
using kCura.IntegrationPoint.Tests.Core.Constants;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.WinEDDS.Service.Export;
using Relativity.Services.Folder;
using Component = Castle.MicroKernel.Registration.Component;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Common.Agent;
using kCura.IntegrationPoints.Core.Installers.Extensions;
using kCura.IntegrationPoints.Core.Toggles;

namespace kCura.IntegrationPoint.Tests.Core.Templates
{
	[TestFixture]
	public abstract class SourceProviderTemplate : IntegrationTestBase
	{
		private readonly string _workspaceName;
		private readonly string _workspaceTemplate;

		protected int RelativityDestinationProviderArtifactId { get; private set; }
		protected IEnumerable<SourceProvider> SourceProviders { get; private set; }
		protected ICaseServiceContext CaseContext { get; private set; }
		protected IRelativityObjectManager ObjectManager { get; private set; }
		protected IIntegrationPointRepository IntegrationPointRepository { get; private set; }
		protected IIntegrationPointService IntegrationPointService { get; private set; }
		protected ISerializer Serializer { get; private set; }
		protected IAPILog Logger { get; private set; }

		protected bool CreatingAgentEnabled { get; set; } = true;
		protected bool CreatingWorkspaceEnabled { get; set; } = true;

		protected int WorkspaceArtifactId { get; private set; }
		protected int AgentArtifactId { get; private set; }

		protected SourceProviderTemplate(
			string workspaceName,
			string workspaceTemplate = WorkspaceTemplateNames.FUNCTIONAL_TEMPLATE_NAME)
		{
			_workspaceName = workspaceName;
			_workspaceTemplate = workspaceTemplate;
		}

		/// <summary>
		/// Use this constructor if you want to run tests versus existing workspace.
		/// </summary>
		/// <param name="workspaceId">Artifact ID of existing workspace.</param>
		protected SourceProviderTemplate(int workspaceId)
		{
			WorkspaceArtifactId = workspaceId;
			CreatingWorkspaceEnabled = false;
		}

		public override void SuiteSetup()
		{
			base.SuiteSetup();

			Manager.Settings.Factory = new HelperConfigSqlServiceFactory(Helper);

			if (CreatingWorkspaceEnabled)
			{
				WorkspaceArtifactId = Workspace.CreateWorkspaceAsync(_workspaceName, _workspaceTemplate)
					.GetAwaiter().GetResult().ArtifactID;
			}

			InitializeIocContainer();

			CaseContext = Container.Resolve<ICaseServiceContext>();
			ObjectManager = CaseContext.RelativityObjectManagerService.RelativityObjectManager;
			IntegrationPointRepository = Container.Resolve<IIntegrationPointRepository>();
			IntegrationPointService = Container.Resolve<IIntegrationPointService>();
			Serializer = Container.Resolve<ISerializer>();
			Logger = Container.Resolve<IAPILog>();

			SourceProviders = GetSourceProviders();
			RelativityDestinationProviderArtifactId = GetRelativityDestinationProviderArtifactId();
		}

		public override void SuiteTeardown()
		{
			if (CreatingWorkspaceEnabled && WorkspaceArtifactId != 0 && !HasTestFailed())
			{
				Workspace.DeleteWorkspaceAsync(WorkspaceArtifactId).GetAwaiter().GetResult();
			}

			base.SuiteTeardown();
		}

		protected virtual void InitializeIocContainer()
		{
			Container.Register(Component
				.For<ILazyComponentLoader>()
				.ImplementedBy<LazyOfTComponentLoader>()
			);

			Container.Register(Component.For<IHelper>().UsingFactoryMethod(k => Helper, managedExternally: true));
			Container.Register(Component.For<IAPILog>().UsingFactoryMethod(k => Helper.GetLoggerFactory().GetLogger()));
			Container.Register(Component.For<IServiceContextHelper>()
				.UsingFactoryMethod(k =>
				{
					IHelper helper = k.Resolve<IHelper>();
					return new TestServiceContextHelper(helper, WorkspaceArtifactId);
				}));
			Container.Register(
				Component.For<IWorkspaceDBContext>()
					.ImplementedBy<WorkspaceDBContext>()
					.UsingFactoryMethod(k => new WorkspaceDBContext(k.Resolve<IHelper>().GetDBContext(WorkspaceArtifactId)))
					.LifeStyle.Transient);

			Container.Register(Component.For<IRelativityObjectManagerService>().Instance(new RelativityObjectManagerService(Container.Resolve<IHelper>(), WorkspaceArtifactId)).LifestyleTransient());
			Container.Register(Component.For<IntegrationPoints.Core.Factories.IExporterFactory>().ImplementedBy<ExporterFactory>());
			Container.Register(Component.For<IExportServiceObserversFactory>().ImplementedBy<ExportServiceObserversFactory>());
			Container.Register(Component.For<IAuthTokenGenerator>().ImplementedBy<ClaimsTokenGenerator>().LifestyleTransient());

			Container.Register(
				Component.For<IFolderManager>().UsingFactoryMethod(f =>
					f.Resolve<IServicesMgr>().CreateProxy<IFolderManager>(ExecutionIdentity.CurrentUser)
				)
			);
			Container.Register(Component.For<FolderWithDocumentsIdRetriever>().ImplementedBy<FolderWithDocumentsIdRetriever>());
			Container.Register(
				Component
					.For<IExternalServiceInstrumentationProvider>()
					.ImplementedBy<ExternalServiceInstrumentationProviderWithoutJobContext>()
					.LifestyleSingleton()
			);
			Container.Register(Component.For<Func<ISearchManager>>()
				.UsingFactoryMethod(k => (Func<ISearchManager>)(() => k.Resolve<IServiceManagerProvider>().Create<ISearchManager, SearchManagerFactory>()))
				.LifestyleTransient()
			);
			Container
				.RegisterWithToggle<IFileRepository, EnableKeplerizedImportAPIToggle, FileRepository, WebAPIFileRepository>(c => c.LifestyleTransient());

			Container.Register(Component.For<IRemovableAgent>().ImplementedBy<FakeNonRemovableAgent>().LifestyleTransient());

#pragma warning disable 618
			var dependencies = new IWindsorInstaller[]
			{
				new QueryInstallers(),
				new KeywordInstaller(),
				new SharedAgentInstaller(),
				new IntegrationPoints.Core.Installers.ServicesInstaller(),
				new ValidationInstaller(),
				new IntegrationPoints.ImportProvider.Parser.Installers.ServicesInstaller()
			};
#pragma warning restore 618

			foreach (IWindsorInstaller dependency in dependencies)
			{
				dependency.Install(Container, ConfigurationStore);
			}
		}

		protected int CreateOrUpdateIntegrationPointRdo(IntegrationPointModel model)
		{
			return IntegrationPointService.SaveIntegration(model);
		}

		protected IntegrationPointModel CreateOrUpdateIntegrationPoint(IntegrationPointModel model)
		{
			int integrationPointArtifactId = CreateOrUpdateIntegrationPointRdo(model);

			IntegrationPoints.Data.IntegrationPoint rdo = IntegrationPointService.ReadIntegrationPoint(integrationPointArtifactId);
			IntegrationPointModel newModel = IntegrationPointModel.FromIntegrationPoint(rdo);
			return newModel;
		}

		protected IntegrationPointProfileModel CreateOrUpdateIntegrationPointProfile(IntegrationPointProfileModel model)
		{
			IIntegrationPointProfileService service = Container.Resolve<IIntegrationPointProfileService>();

			int integrationPointArtifactId = service.SaveIntegration(model);

			IntegrationPointProfile rdo = service.ReadIntegrationPointProfile(integrationPointArtifactId);
			IntegrationPointProfileModel newModel = IntegrationPointProfileModel.FromIntegrationPointProfile(rdo);
			return newModel;
		}
        
		private IEnumerable<SourceProvider> GetSourceProviders()
		{
			var queryRequest = new QueryRequest();
			List<SourceProvider> sourceProviders = ObjectManager.Query<SourceProvider>(queryRequest);
			return sourceProviders;
		}

		private int GetRelativityDestinationProviderArtifactId()
		{
			var queryRequestForIdentifierField = new QueryRequest
			{
				Fields = new List<FieldRef>
				{
					new FieldRef {Guid = DestinationProviderFieldGuids.IdentifierGuid}
				}
			};

			return ObjectManager
				.Query<DestinationProvider>(queryRequestForIdentifierField)
				.First(x => x.Identifier == IntegrationPoints.Core.Constants.IntegrationPoints.RELATIVITY_DESTINATION_PROVIDER_GUID)
				.ArtifactId;
		}
	}
}
