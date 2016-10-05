using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Mvc;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Config;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Domain;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Factories.Implementations;
using kCura.IntegrationPoints.Core.Helpers;
using kCura.IntegrationPoints.Core.Helpers.Implementations;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Queries;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Services.Synchronizer;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Contexts;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Data.Queries;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Readers;
using kCura.IntegrationPoints.Domain.Synchronizer;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Authentication;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Helpers;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Process;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Services;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary;
using kCura.IntegrationPoints.FtpProvider.Connection;
using kCura.IntegrationPoints.FtpProvider.Connection.Interfaces;
using kCura.IntegrationPoints.FtpProvider.Helpers;
using kCura.IntegrationPoints.FtpProvider.Helpers.Interfaces;
using kCura.IntegrationPoints.ImportProvider.Parser.Services;
using kCura.IntegrationPoints.ImportProvider.Parser.Services.Interfaces;
using kCura.IntegrationPoints.Security;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.IntegrationPoints.Web.Attributes;
using kCura.Relativity.Client;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Services;
using Microsoft.AspNet.SignalR.Hubs;
using Relativity.API;
using Relativity.Core.Service;
using Relativity.CustomPages;
using Relativity.Toggles;
using Relativity.Toggles.Providers;
using SystemInterface.IO;

namespace kCura.IntegrationPoints.Web.Installers
{
    public class ControllerInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            #region Conventions

            container.Register(Classes.FromThisAssembly().BasedOn<IController>().LifestyleTransient());
            container.Register(Classes.FromThisAssembly().BasedOn<IHub>().LifestyleTransient());

            container.Register(Classes.FromThisAssembly().BasedOn<IHttpController>().LifestyleTransient());

            #endregion

            container.Register(Component.For<IWorkspaceService>().ImplementedBy<ControllerCustomPageService>().LifestyleTransient());
            container.Register(Component.For<IWorkspaceService>().ImplementedBy<WebAPICustomPageService>().LifestyleTransient());

            if (container.Kernel.HasComponent(typeof(IConfig)) == false)
            {
                container.Register(Component.For<IConfig>().Instance(Config.Config.Instance));
            }

            container.Register(Component.For<IConfigFactory>().ImplementedBy<ConfigFactory>().LifestyleTransient());
            container.Register(Component.For<IServiceManagerProvider>().ImplementedBy<ServiceManagerProvider>().LifestyleTransient());
            container.Register(Component.For<ISessionService>().UsingFactoryMethod(k => SessionService.Session).LifestylePerWebRequest());
            container.Register(Component.For<WebClientFactory>().ImplementedBy<WebClientFactory>().LifestyleTransient());

            if (container.Kernel.HasComponent(typeof(Apps.Common.Utils.Serializers.ISerializer)) == false)
            {
                container.Register(Component.For<kCura.Apps.Common.Utils.Serializers.ISerializer>().ImplementedBy<kCura.Apps.Common.Utils.Serializers.JSONSerializer>().LifestyleTransient());
            }
            container.Register(Component.For<IHelper>().UsingFactoryMethod((k) => ConnectionHelper.Helper()).LifestylePerWebRequest());
            container.Register(Component.For<ICPHelper>().UsingFactoryMethod((k) => ConnectionHelper.Helper()).LifestylePerWebRequest());
            container.Register(Component.For<IServiceContextHelper>().ImplementedBy<ServiceContextHelperForWeb>().LifestylePerWebRequest());
            container.Register(Component.For<ICaseServiceContext>().ImplementedBy<CaseServiceContext>().LifestylePerWebRequest());
            container.Register(Component.For<IEddsServiceContext>().ImplementedBy<EddsServiceContext>().LifestyleTransient());
            container.Register(Component.For<IJobService>().ImplementedBy<JobService>().LifestyleTransient());
            container.Register(
                Component.For<Data.IWorkspaceDBContext>()
                    .ImplementedBy<Data.WorkspaceContext>()
                    .UsingFactoryMethod((k) => new WorkspaceContext(k.Resolve<WebClientFactory>().CreateDbContext()))
                    .LifeStyle.Transient);

            var guid = Guid.Parse(GlobalConst.RELATIVITY_INTEGRATION_POINTS_AGENT_GUID);
            container.Register(
                Component.For<IAgentService>()
                    .ImplementedBy<AgentService>()
                    .DependsOn(Dependency.OnValue<Guid>(guid))
                    .LifestyleTransient());

            container.AddFacility<TypedFactoryFacility>();
            container.Register(Component.For<IErrorFactory>().AsFactory().UsingFactoryMethod((k) => new ErrorFactory(container)));
            container.Register(Component.For<WebAPIFilterException>().ImplementedBy<WebAPIFilterException>());

            container.Register(Component.For<IRSAPIClient>().UsingFactoryMethod((k) =>
                k.Resolve<WebClientFactory>().CreateClient()).LifestyleTransient());

            container.Register(Component.For<global::Relativity.API.IDBContext>().UsingFactoryMethod((k) =>
                k.Resolve<WebClientFactory>().CreateDbContext()).LifestyleTransient());

            container.Register(Component.For<IServicesMgr>().UsingFactoryMethod((k) =>
                k.Resolve<WebClientFactory>().CreateServicesMgr()).LifestyleTransient());

            container.Register(Component.For<GridModelFactory>().ImplementedBy<GridModelFactory>().LifestyleTransient());

            container.Register(
                Component.For<GetApplicationBinaries>()
                    .ImplementedBy<GetApplicationBinaries>().DynamicParameters((k, d) => d["eddsDBcontext"] = ConnectionHelper.Helper().GetDBContext(-1))
                    .LifeStyle.Transient);

            container.Register(Component.For<IRelativityUrlHelper>().ImplementedBy<RelativityUrlHelper>().LifeStyle.Transient);

            container.Register(Component.For<WebAPILoginException>().ImplementedBy<WebAPILoginException>().LifeStyle.Transient);

            // TODO: we need to make use of an async GetDBContextAsync (pending Dan Wells' patch) -- biedrzycki: Feb 5th, 2016
            container.Register(Component.For<IToggleProvider>().Instance(new SqlServerToggleProvider(
                () =>
                {
                    SqlConnection connection = ConnectionHelper.Helper().GetDBContext(-1).GetConnection(true);

                    return connection;
                },
                async () =>
                {
                    Task<SqlConnection> task = Task.Run(() =>
                    {
                        SqlConnection connection = ConnectionHelper.Helper().GetDBContext(-1).GetConnection(true);
                        return connection;
                    });

                    return await task;
                })).LifestyleTransient());

            container.Register(Component.For<IRepositoryFactory>().ImplementedBy<RepositoryFactory>().LifestyleSingleton());

            container.Register(Component.For<IWorkspaceRepository>()
                    .ImplementedBy<KeplerWorkspaceRepository>()
                    .UsingFactoryMethod((k) => k.Resolve<IRepositoryFactory>().GetWorkspaceRepository())
                    .LifestyleTransient());

            container.Register(Component.For<IHtmlSanitizerManager>().ImplementedBy<HtmlSanitizerManager>().LifestyleSingleton());

            container.Register(Component.For<IOnBehalfOfUserClaimsPrincipalFactory>().ImplementedBy<OnBehalfOfUserClaimsPrincipalFactory>().LifestyleTransient());

            container.Register(Component.For<IExportFieldsService>().ImplementedBy<ExportFieldsService>().LifestyleTransient());
            container.Register(Component.For<IViewService>().ImplementedBy<ViewService>().LifestyleTransient());
            container.Register(Component.For<IProductionService>().ImplementedBy<ProductionService>().LifestyleTransient());
            container.Register(Component.For<IArtifactTreeService>().ImplementedBy<ArtifactTreeService>().LifestyleTransient());
            container.Register(Component.For<IExportSettingsValidationService>().ImplementedBy<ExportSettingsValidationService>().LifestyleTransient());
            container.Register(Component.For<IPaddingValidator>().ImplementedBy<PaddingValidator>().LifestyleTransient());
            container.Register(Component.For<IExportSettingsBuilder>().ImplementedBy<ExportSettingsBuilder>().LifestyleTransient());
            container.Register(Component.For<IExportFileBuilder>().ImplementedBy<ExportFileBuilder>().LifestyleTransient());
            container.Register(Component.For<IDelimitersBuilder>().ImplementedBy<DelimitersBuilder>().LifestyleTransient());
            container.Register(Component.For<IVolumeInfoBuilder>().ImplementedBy<VolumeInfoBuilder>().LifestyleTransient());
            container.Register(Component.For<ISavedSearchesTreeService>().ImplementedBy<SavedSearchesTreeService>().LifestyleTransient());

            #region FTP Provider

            container.Register(Component.For<IConnectorFactory>().ImplementedBy<ConnectorFactory>().LifestyleSingleton().OnlyNewServices());
            container.Register(
                Component.For<ISettingsManager>().ImplementedBy<SettingsManager>().LifestyleTransient().OnlyNewServices());
            container.Register(Component.For<ICredentialProvider>().ImplementedBy<TokenCredentialProvider>());

            #endregion

            #region Import Provider

            container.Register(Component.For<IImportPreviewService>().ImplementedBy<ImportPreviewService>());

            #endregion

            #region Core

            const string CORE_ASSEMBLY_NAME = "kCura.IntegrationPoints.Core";

            #region Convention

            // register intefaceless classes :(
            var interfacelessServicesToExclude = new HashSet<string>(new[]
            {
                typeof(DeleteHistoryService).Name,
                typeof(DeleteIntegrationPoints).Name,
            });
            container.Register(
                Classes.FromAssemblyNamed(CORE_ASSEMBLY_NAME)
                    .InNamespace("kCura.IntegrationPoints.Core.Services", true)
                    .If(x => !x.GetInterfaces().Any())
                    .If(x => !interfacelessServicesToExclude.Contains(x.Name))
                    .Configure(c => c.LifestyleTransient()));

            var servicesToExclude = new HashSet<string>(new[]
            {
                typeof(DeleteHistoryErrorService).Name,
                typeof(JobStatusUpdater).Name,
                typeof(GeneralWithCustodianRdoSynchronizerFactory).Name,
                typeof(ExportDestinationSynchronizerFactory).Name
            });
            var namespacesToExclude = new HashSet<string>(
                new[]
                {
                    "kCura.IntegrationPoints.Core.Services.Exporter",
                    "kCura.IntegrationPoints.Core.Services.Keywords",
                    "kCura.IntegrationPoints.Core.Services.ServiceContext"
                }
            );

            container.Register(
                Classes.FromAssemblyNamed(CORE_ASSEMBLY_NAME)
                    .InNamespace("kCura.IntegrationPoints.Core.Services", true)
                    .If(x => x.GetInterfaces().Any())
                    .If(x => !servicesToExclude.Contains(x.Name))
                    .If(x => x.Namespace != null && !namespacesToExclude.Contains(x.Namespace))
                    .WithService.DefaultInterfaces());

            container.Register(
                Classes.FromAssemblyNamed(CORE_ASSEMBLY_NAME)
                    .InNamespace("kCura.IntegrationPoints.Core.Domain", true)
                    .If(x => x.GetInterfaces().Any())
                    .If(x => x.Name != typeof(AppDomainFactory).Name)
                    .WithService.DefaultInterfaces());

            #endregion

            container.Register(Component.For<IEncryptionManager>().ImplementedBy<DefaultEncryptionManager>().LifestyleSingleton());
            container.Register(Component.For<RsapiClientFactory>().ImplementedBy<RsapiClientFactory>().LifestyleTransient());
            container.Register(Component.For<IContextContainerFactory>().ImplementedBy<ContextContainerFactory>().LifestyleSingleton());
            container.Register(Component.For<IManagerFactory>().ImplementedBy<ManagerFactory>().LifestyleTransient());
            container.Register(Component.For<IDataSynchronizer>().ImplementedBy<RdoSynchronizerPush>().Named(typeof(RdoSynchronizerPush).AssemblyQualifiedName).LifeStyle.Transient);
            container.Register(Component.For<IDataSynchronizer>().ImplementedBy<RdoSynchronizerPull>().Named(typeof(RdoSynchronizerPull).AssemblyQualifiedName).LifeStyle.Transient);
            container.Register(Component.For<IDataSynchronizer>().ImplementedBy<RdoCustodianSynchronizer>().Named(typeof(RdoCustodianSynchronizer).AssemblyQualifiedName).LifeStyle.Transient);
            container.Register(
                Component.For<IDataSynchronizer>()
                    .ImplementedBy<ExportSynchroznizer>()
                    .Named(typeof(ExportSynchroznizer).AssemblyQualifiedName)
                    .LifeStyle.Transient);

            container.Register(Component.For<ISynchronizerFactory>().ImplementedBy<GeneralWithCustodianRdoSynchronizerFactory>().DependsOn(new { container = container }).LifestyleTransient());
            container.Register(Component.For<ISynchronizerFactory>().ImplementedBy<ExportDestinationSynchronizerFactory>().DependsOn(new { container = container }).LifestyleTransient());
            container.Register(Component.For<IDataProviderFactory>().ImplementedBy<AppDomainFactory>().LifestyleTransient());
            container.Register(Component.For<RdoFilter>().ImplementedBy<RdoFilter>().LifestyleTransient());
            container.Register(
                Component.For<GetSourceProviderRdoByIdentifier>()
                    .ImplementedBy<GetSourceProviderRdoByIdentifier>()
                    .LifeStyle.Transient);

			container.Register(Component.For<IDirectoryTreeCreator<JsTreeItemDTO>>().ImplementedBy<DirectoryTreeCreator<JsTreeItemDTO>>().LifestyleTransient());
            container.Register(Component.For<IArtifactTreeCreator>().ImplementedBy<ArtifactTreeCreator>().LifestyleTransient());
            container.Register(Component.For<ISavedSearchesTreeCreator>().ImplementedBy<SavedSearchesTreeCreator>());
            container.Register(Component.For<IResourcePoolManager>().ImplementedBy<ResourcePoolManager>().LifestyleTransient());
			container.Register(Component.For<IDirectory>().ImplementedBy<LongPathDirectory>().LifestyleTransient());

            #endregion

            #region Domain

            const string DOMAIN_ASSEMBLY_NAME = "kCura.IntegrationPoints.Domain";

            #region Convention

            var excludedNamespaces = new HashSet<string>(new[]
            {
                "kCura.IntegrationPoints.Domain.Models"
            });
            var excludedClasses = new HashSet<string>(new[]
            {
                typeof(DataColumnWithValue).Name
            });
            container.Register(Classes.FromAssemblyNamed(DOMAIN_ASSEMBLY_NAME)
                .Pick()
                .If(x => !excludedNamespaces.Contains(x.Namespace))
                .If(x => !excludedClasses.Contains(x.Name))
                .WithService.DefaultInterfaces());

            #endregion

            #endregion

            #region Data

            const string DATA_ASSEMBLY_NAME = "kCura.IntegrationPoints.Data";

            #region Convention

            HashSet<string> queryObjectsToExclude = new HashSet<string>(
                new[]
                {
                    typeof (GetApplicationBinaries).Name,
                    typeof (JobHistoryErrorQuery).Name,
                });
            container.Register(
                Classes.FromAssemblyNamed(DATA_ASSEMBLY_NAME)
                    .InNamespace("kCura.IntegrationPoints.Data.Queries")
                    .If(x => !x.GetInterfaces().Any())
                    .If(x => !queryObjectsToExclude.Contains(x.Name))
                    .Configure(c => c.LifestyleTransient()));

            #endregion

            container.Register(Component.For<IObjectTypeQuery>().ImplementedBy<SqlObjectTypeQuery>().LifestyleTransient());
            container.Register(Component.For<RSAPIRdoQuery>().ImplementedBy<RSAPIRdoQuery>().LifeStyle.Transient);

            container.Register(Component.For<IChoiceQuery>().ImplementedBy<ChoiceQuery>().LifeStyle.Transient);
            container.Register(Component.For<IFileQuery>().ImplementedBy<kCura.IntegrationPoints.Data.Queries.FileQuery>().LifeStyle.Transient);

            #endregion

            #region Synchronizer

            container.Register(Component.For<IImportApiFactory>().ImplementedBy<ImportApiFactory>().LifeStyle.Transient);
            container.Register(Component.For<IRelativityFieldQuery>().ImplementedBy<RelativityFieldQuery>().LifestyleTransient());

            #endregion
        }
    }
}