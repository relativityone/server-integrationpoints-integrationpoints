using System;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Validation.Parts;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Process;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Validation.Parts;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Validation
{
	public interface IFileDestinationProviderValidatorsFactory
	{
		ExportFileValidator CreateExportFileValidator();

		WorkspaceValidator CreateWorkspaceValidator();

		SavedSearchValidator CreateSavedSearchValidator(int workspaceArtifactId, int savedSearchArtifactId);

		ViewValidator CreateViewValidator();

		ProductionValidator CreateProductionValidator();
	}

	public class FileDestinationProviderValidatorsFactory : IFileDestinationProviderValidatorsFactory
	{
		private readonly ISerializer _serializer;
		private readonly IExportSettingsBuilder _exportSettingsBuilder;
		private readonly IExportInitProcessService _exportInitProcessService;
		private readonly IExportFileBuilder _exportFileBuilder;
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly IProductionService _productionService;
		private readonly IViewService _viewService;

		public FileDestinationProviderValidatorsFactory(
			ISerializer serializer,
			IExportSettingsBuilder exportSettingsBuilder,
			IExportInitProcessService exportInitProcessService,
			IExportFileBuilder exportFileBuilder,
			IRepositoryFactory repositoryFactory,
			IProductionService productionService,
			IViewService viewService
		)
		{
			_serializer = serializer;
			_exportSettingsBuilder = exportSettingsBuilder;
			_exportInitProcessService = exportInitProcessService;
			_exportFileBuilder = exportFileBuilder;
			_repositoryFactory = repositoryFactory;
			_productionService = productionService;
			_viewService = viewService;
		}

		public ExportFileValidator CreateExportFileValidator()
		{
			return new ExportFileValidator(_serializer, _exportSettingsBuilder, _exportInitProcessService, _exportFileBuilder);
		}

		public WorkspaceValidator CreateWorkspaceValidator()
		{
			return new WorkspaceValidator(_repositoryFactory.GetWorkspaceRepository(), "Source");
		}

		public SavedSearchValidator CreateSavedSearchValidator(int workspaceArtifactId, int savedSearchArtifactId)
		{
			return new SavedSearchValidator(_repositoryFactory.GetSavedSearchRepository(workspaceArtifactId, savedSearchArtifactId));
		}

		public ViewValidator CreateViewValidator()
		{
			return new ViewValidator(_viewService);
		}

		public ProductionValidator CreateProductionValidator()
		{
			return new ProductionValidator(_productionService);
		}
	}
}