﻿using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Validation.Helpers;
using kCura.IntegrationPoints.Core.Validation.Parts;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Process;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Validation.Parts;
using kCura.Relativity.Client;
using Relativity.API;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Validation
{
	public interface IFileDestinationProviderValidatorsFactory
	{
		ExportFileValidator CreateExportFileValidator();

		WorkspaceValidator CreateWorkspaceValidator();

		SavedSearchValidator CreateSavedSearchValidator(int workspaceArtifactId, int savedSearchArtifactId);

		ViewValidator CreateViewValidator();

		ExportProductionValidator CreateExportProductionValidator();

		ExportNativeSettingsValidator CreateExportNativeSettingsValidator();

		ArtifactValidator CreateArtifactValidator(int workspaceArtifactId, string artifactTypeName);

		FieldsMapValidator CreateFieldsMapValidator();

		BaseExportSettingsValidator CreateExportSettingsValidator(int artifactTypeId);
	}

	public class FileDestinationProviderValidatorsFactory : IFileDestinationProviderValidatorsFactory
	{
		private readonly ISerializer _serializer;
		private readonly IExportSettingsBuilder _exportSettingsBuilder;
		private readonly IExportInitProcessService _exportInitProcessService;
		private readonly IExportFileBuilder _exportFileBuilder;
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly IHelper _helper;
		private readonly IContextContainerFactory _contextContainerFactory;
		private readonly IManagerFactory _managerFactory;
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
			IContextContainerFactory contextContainerFactory,
			IManagerFactory managerFactory,
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
			_helper = helper;
			_contextContainerFactory = contextContainerFactory;
			_managerFactory = managerFactory;
			_viewService = viewService;
			_exportFieldsService = exportFieldsService;
			_artifactService = artifactService;
			_nonValidCharactersValidator = nonValidCharactersValidator;
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
			return new SavedSearchValidator(_repositoryFactory.GetSavedSearchQueryRepository(workspaceArtifactId), savedSearchArtifactId);
		}

		public ViewValidator CreateViewValidator()
		{
			return new ViewValidator(_viewService);
		}

		public ExportProductionValidator CreateExportProductionValidator()
		{
			IContextContainer contextContainer = _contextContainerFactory.CreateContextContainer(_helper);
			IProductionManager productionManager = _managerFactory.CreateProductionManager(contextContainer);

			return new ExportProductionValidator(productionManager);
		}

		public ArtifactValidator CreateArtifactValidator(int workspaceArtifactId, string artifactTypeName)
		{
			return new ArtifactValidator(_artifactService, workspaceArtifactId, artifactTypeName);
		}

		public FieldsMapValidator CreateFieldsMapValidator()
		{
			return new FieldsMapValidator(_serializer, _exportFieldsService);
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