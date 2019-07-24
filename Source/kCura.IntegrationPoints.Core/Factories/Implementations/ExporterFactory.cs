using System.Security.Claims;
using System.Text;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services.Exporter;
using kCura.IntegrationPoints.Core.Services.Exporter.Images;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data.Contexts;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Newtonsoft.Json;
using Relativity.API;

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
			string userImportApiSettings,
			IDocumentRepository documentRepository,
			ISerializer serializer)
		{
			LogBuildExporterExecutionWithParameters(mappedFields, serializedSourceConfiguration, savedSearchArtifactID, onBehalfOfUser, userImportApiSettings);
			ClaimsPrincipal claimsPrincipal = GetClaimsPrincipal(onBehalfOfUser);
			IBaseServiceContextProvider baseServiceContextProvider = new BaseServiceContextProvider(claimsPrincipal);

			ImportSettings settings = JsonConvert.DeserializeObject<ImportSettings>(userImportApiSettings);
			SourceConfiguration sourceConfiguration = JsonConvert.DeserializeObject<SourceConfiguration>(serializedSourceConfiguration);

			IExporterService exporter = settings.ImageImport ?
				CreateImageExporterService(
					jobStopManager,
					mappedFields,
					savedSearchArtifactID,
					baseServiceContextProvider,
					settings,
					sourceConfiguration,
					documentRepository) :
				CreateRelativityExporterService(
					jobStopManager,
					mappedFields,
					serializedSourceConfiguration,
					savedSearchArtifactID,
					baseServiceContextProvider,
					settings,
					documentRepository,
					serializer);
			return exporter;
		}
		
		private IExporterService CreateRelativityExporterService(
			IJobStopManager jobStopManager,
			FieldMap[] mappedFields,
			string serializedSourceConfiguration,
			int savedSearchArtifactID,
			IBaseServiceContextProvider baseServiceContextProvider,
			ImportSettings settings,
			IDocumentRepository documentRepository,
			ISerializer serializer)
		{
			SourceConfiguration sourceConfiguration = serializer.Deserialize<SourceConfiguration>(serializedSourceConfiguration);
			int workspaceArtifactID = sourceConfiguration.SourceWorkspaceArtifactId;
			bool useDynamicFolderPath = settings.UseDynamicFolderPath;
			IFolderPathReader folderPathReader = _folderPathReaderFactory.Create(workspaceArtifactID, useDynamicFolderPath);
			const int startAtRecord = 0;

			return new RelativityExporterService(
				documentRepository,
				_relativityObjectManager,
				_sourceRepositoryFactory,
				_targetRepositoryFactory,
				jobStopManager,
				_helper,
				folderPathReader,
				baseServiceContextProvider,
				mappedFields,
				startAtRecord,
				sourceConfiguration,
				savedSearchArtifactID);
		}

		private IExporterService CreateImageExporterService(
			IJobStopManager jobStopManager,
			FieldMap[] mappedFiles,
			int savedSearchArtifactId,
			IBaseServiceContextProvider baseServiceContextProvider,
			ImportSettings settings,
			SourceConfiguration sourceConfiguration,
			IDocumentRepository documentRepository)
		{
			int searchArtifactId;
			if (sourceConfiguration.TypeOfExport == SourceConfiguration.ExportType.SavedSearch)
			{
				searchArtifactId = savedSearchArtifactId;
			}
			else
			{
				searchArtifactId = sourceConfiguration.SourceProductionId;
			}

			const int startAtRecord = 0;
			return new ImageExporterService(
				documentRepository,
				_relativityObjectManager,
				_sourceRepositoryFactory,
				_targetRepositoryFactory,
				jobStopManager,
				_helper,
				baseServiceContextProvider,
				mappedFiles,
				startAtRecord,
				sourceConfiguration,
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