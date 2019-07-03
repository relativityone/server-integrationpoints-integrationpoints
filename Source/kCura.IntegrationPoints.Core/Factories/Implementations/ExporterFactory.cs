using System.Security.Claims;
using System.Text;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services.Exporter;
using kCura.IntegrationPoints.Core.Services.Exporter.Images;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data.Contexts;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Newtonsoft.Json;
using Relativity.API;
using Relativity.Core;
using Relativity.Core.Api.Shared.Manager.Export;

namespace kCura.IntegrationPoints.Core.Factories.Implementations
{
	public class ExporterFactory : IExporterFactory
	{
		private const int _ADMIN_USER_ID = 9;
		private readonly IOnBehalfOfUserClaimsPrincipalFactory _claimsPrincipalFactory;
		private readonly IRepositoryFactory _sourceRepositoryFactory;
		private readonly IRepositoryFactory _targetRepositoryFactory;
		private readonly IHelper _helper;
		private readonly IFolderPathReaderFactory _folderPathReaderFactory;
		private readonly IRelativityObjectManager _relativityObjectManager;
		private readonly IAPILog _logger;

		public ExporterFactory(
			IOnBehalfOfUserClaimsPrincipalFactory claimsPrincipalFactory,
			IRepositoryFactory sourceRepositoryFactory,
			IRepositoryFactory targetRepositoryFactory,
			IHelper helper,
			IFolderPathReaderFactory folderPathReaderFactory,
			IRelativityObjectManager relativityObjectManager)
		{
			_claimsPrincipalFactory = claimsPrincipalFactory;
			_sourceRepositoryFactory = sourceRepositoryFactory;
			_targetRepositoryFactory = targetRepositoryFactory;
			_helper = helper;
			_folderPathReaderFactory = folderPathReaderFactory;
			_relativityObjectManager = relativityObjectManager;
			_logger = _helper.GetLoggerFactory().GetLogger().ForContext<ExporterFactory>();
		}

		public IExporterService BuildExporter(
			IJobStopManager jobStopManager,
			FieldMap[] mappedFields,
			string serializedSourceConfiguration,
			int savedSearchArtifactID,
			int onBehalfOfUser,
			string userImportApiSettings)
		{
			LogBuildExporterExecutionWithParameters(mappedFields, serializedSourceConfiguration, savedSearchArtifactID, onBehalfOfUser, userImportApiSettings);
			ClaimsPrincipal claimsPrincipal = GetClaimsPrincipal(onBehalfOfUser);
			IBaseServiceContextProvider baseServiceContextProvider = new BaseServiceContextProvider(claimsPrincipal);

			ImportSettings settings = JsonConvert.DeserializeObject<ImportSettings>(userImportApiSettings);
			SourceConfiguration sourceConfiguration = JsonConvert.DeserializeObject<SourceConfiguration>(serializedSourceConfiguration);
			BaseServiceContext baseServiceContext = claimsPrincipal.GetUnversionContext(sourceConfiguration.SourceWorkspaceArtifactId);

			IExporterService exporter = settings.ImageImport ?
				CreateImageExporterService(
					jobStopManager,
					mappedFields,
					serializedSourceConfiguration,
					savedSearchArtifactID,
					baseServiceContextProvider,
					settings,
					sourceConfiguration,
					baseServiceContext) :
				CreateRelativityExporterService(
					jobStopManager,
					mappedFields,
					serializedSourceConfiguration,
					savedSearchArtifactID,
					claimsPrincipal,
					baseServiceContextProvider,
					settings,
					baseServiceContext);
			return exporter;
		}
		
		private IExporterService CreateRelativityExporterService(
			IJobStopManager jobStopManager,
			FieldMap[] mappedFields,
			string config,
			int savedSearchArtifactId,
			ClaimsPrincipal claimsPrincipal,
			IBaseServiceContextProvider baseServiceContextProvider,
			ImportSettings settings,
			BaseServiceContext baseServiceContext)
		{
			IExporter exporter = BuildSavedSearchExporter(baseServiceContext, settings.LoadImportedFullTextFromServer);
			IFolderPathReader folderPathReader = _folderPathReaderFactory.Create(claimsPrincipal, settings, config);
			const int startAtRecord = 0;

			return new RelativityExporterService(
				exporter,
				_relativityObjectManager,
				_sourceRepositoryFactory,
				_targetRepositoryFactory,
				jobStopManager,
				_helper,
				folderPathReader,
				baseServiceContextProvider,
				mappedFields,
				startAtRecord,
				config,
				savedSearchArtifactId);
		}

		private IExporterService CreateImageExporterService(
			IJobStopManager jobStopManager,
			FieldMap[] mappedFiles,
			string config,
			int savedSearchArtifactId,
			IBaseServiceContextProvider baseServiceContextProvider,
			ImportSettings settings,
			SourceConfiguration sourceConfiguration,
			BaseServiceContext baseServiceContext)
		{
			IExporter exporter;
			int searchArtifactId;
			if (sourceConfiguration.TypeOfExport == SourceConfiguration.ExportType.SavedSearch)
			{
				exporter = BuildSavedSearchExporter(baseServiceContext, settings.LoadImportedFullTextFromServer);
				searchArtifactId = savedSearchArtifactId;
			}
			else
			{
				exporter = BuildProductionExporter(baseServiceContext, settings.LoadImportedFullTextFromServer);
				searchArtifactId = sourceConfiguration.SourceProductionId;
			}

			const int startAtRecord = 0;
			return new ImageExporterService(
				exporter,
				_relativityObjectManager,
				_sourceRepositoryFactory,
				_targetRepositoryFactory,
				jobStopManager,
				_helper,
				baseServiceContextProvider,
				mappedFiles,
				startAtRecord,
				config,
				searchArtifactId,
				settings);
		}

		private ClaimsPrincipal GetClaimsPrincipal(int onBehalfOfUser)
		{
			if (onBehalfOfUser == 0)
			{
				onBehalfOfUser = _ADMIN_USER_ID;
			}
			ClaimsPrincipal claimsPrincipal = _claimsPrincipalFactory.CreateClaimsPrincipal(onBehalfOfUser);
			return claimsPrincipal;
		}

		private IExporter BuildSavedSearchExporter(BaseServiceContext baseService, bool shouldUseDgPaths)
		{
			return new SavedSearchExporter(
				baseService,
				new UserPermissionsMatrix(baseService),
				global::Relativity.ArtifactType.Document,
				Domain.Constants.MULTI_VALUE_DELIMITER,
				Domain.Constants.NESTED_VALUE_DELIMITER,
				global::Relativity.Core.Api.Settings.RSAPI.Config.DynamicallyLoadedDllPaths,
				shouldUseDgPaths);
		}

		private IExporter BuildProductionExporter(BaseServiceContext baseService, bool shouldUseDgPaths)
		{
			return new ProductionExporter(
				baseService,
				new UserPermissionsMatrix(baseService),
				global::Relativity.ArtifactType.Document,
				Domain.Constants.MULTI_VALUE_DELIMITER,
				Domain.Constants.NESTED_VALUE_DELIMITER,
				global::Relativity.Core.Api.Settings.RSAPI.Config.DynamicallyLoadedDllPaths,
				shouldUseDgPaths);
		}

		private void LogBuildExporterExecutionWithParameters(
			FieldMap[] mappedFields,
			string config,
			int savedSearchArtifactId,
			int onBehalfOfUser,
			string userImportApiSettings)
		{
			var msgBuilder = new StringBuilder("Building Exporter with parameters: \n");
			msgBuilder.AppendLine("mappedFields {@mappedFields} ");
			msgBuilder.AppendLine("config {config} ");
			msgBuilder.AppendLine("savedSearchArtifactId {savedSearchArtifactId} ");
			msgBuilder.AppendLine("onBehalfOfUser: {onBehalfOfUser} ");
			msgBuilder.AppendLine("userImportApiSettings {userImportApiSettings}");
			string msgTemplate = msgBuilder.ToString();
			_logger.LogDebug(
				msgTemplate,
				mappedFields,
				config,
				savedSearchArtifactId,
				onBehalfOfUser,
				userImportApiSettings);
		}
	}
}