using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Validation.Helpers;
using kCura.IntegrationPoints.Core.Validation.Parts;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Process;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Validation.Parts;
using Relativity;
using Relativity.API;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Validation
{
    public interface IFileDestinationProviderValidatorsFactory
    {
        ExportFileValidator CreateExportFileValidator();

        WorkspaceValidator CreateWorkspaceValidator();

        SavedSearchValidator CreateSavedSearchValidator(int workspaceArtifactId, int savedSearchArtifactId);

        ViewExportValidator CreateViewValidator();

        ExportProductionValidator CreateExportProductionValidator();

        ExportNativeSettingsValidator CreateExportNativeSettingsValidator();

        ArtifactValidator CreateArtifactValidator(int workspaceArtifactId, string artifactTypeName);

        FieldsMapValidator CreateFieldsMapValidator();

        BaseExportSettingsValidator CreateExportSettingsValidator(int artifactTypeId);
    }

    public class FileDestinationProviderValidatorsFactory : IFileDestinationProviderValidatorsFactory
    {
        private readonly IAPILog _logger;
        private readonly ISerializer _serializer;
        private readonly IExportSettingsBuilder _exportSettingsBuilder;
        private readonly IExportInitProcessService _exportInitProcessService;
        private readonly IExportFileBuilder _exportFileBuilder;
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly IProductionManager _productionManager;
        private readonly IViewService _viewService;
        private readonly IExportFieldsService _exportFieldsService;
        private readonly IArtifactService _artifactService;
        private readonly INonValidCharactersValidator _nonValidCharactersValidator;

        public FileDestinationProviderValidatorsFactory(
            ISerializer serializer,
            IExportSettingsBuilder exportSettingsBuilder,
            IExportInitProcessService exportInitProcessService,
            IExportFileBuilder exportFileBuilder,
            IRepositoryFactory repositoryFactory,
            IHelper helper,
            IProductionManager productionManager,
            IViewService viewService,
            IExportFieldsService exportFieldsService,
            IArtifactService artifactService,
            INonValidCharactersValidator nonValidCharactersValidator
        )
        {
            _serializer = serializer;
            _exportSettingsBuilder = exportSettingsBuilder;
            _exportInitProcessService = exportInitProcessService;
            _exportFileBuilder = exportFileBuilder;
            _repositoryFactory = repositoryFactory;
            _productionManager = productionManager;
            _viewService = viewService;
            _exportFieldsService = exportFieldsService;
            _artifactService = artifactService;
            _nonValidCharactersValidator = nonValidCharactersValidator;

            _logger = helper.GetLoggerFactory().GetLogger();
        }

        public ExportFileValidator CreateExportFileValidator()
        {
            return new ExportFileValidator(_serializer, _exportSettingsBuilder, _exportInitProcessService, _exportFileBuilder);
        }

        public ExportNativeSettingsValidator CreateExportNativeSettingsValidator()
        {
            return new ExportNativeSettingsValidator(_serializer, _exportSettingsBuilder, _exportFileBuilder, _exportFieldsService);
        }

        public WorkspaceValidator CreateWorkspaceValidator()
        {
            return new WorkspaceValidator(_repositoryFactory.GetWorkspaceRepository(), "Source");
        }

        public SavedSearchValidator CreateSavedSearchValidator(int workspaceArtifactId, int savedSearchArtifactId)
        {
            return new SavedSearchValidator(_logger, _repositoryFactory.GetSavedSearchQueryRepository(workspaceArtifactId));
        }

        public ViewExportValidator CreateViewValidator()
        {
            return new ViewExportValidator(_logger, _viewService);
        }

        public ExportProductionValidator CreateExportProductionValidator()
        {
            return new ExportProductionValidator(_productionManager);
        }

        public ArtifactValidator CreateArtifactValidator(int workspaceArtifactId, string artifactTypeName)
        {
            return new ArtifactValidator(_artifactService, workspaceArtifactId, artifactTypeName);
        }

        public FieldsMapValidator CreateFieldsMapValidator()
        {
            return new FieldsMapValidator(_logger, _serializer, _exportFieldsService);
        }

        public BaseExportSettingsValidator CreateExportSettingsValidator(int artifactTypeId)
        {
            switch (artifactTypeId)
            {
                case (int)ArtifactType.Document:
                    return new DocumentExportSettingsValidator(_nonValidCharactersValidator);

                default:
                    return new RdoExportSettingsValidator(_nonValidCharactersValidator);
            }
        }
    }
}
