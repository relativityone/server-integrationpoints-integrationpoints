using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services.Exporter.Base;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data.DTO;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Readers;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Relativity.API;
using ArtifactType = kCura.Relativity.Client.ArtifactType;
using DataView = kCura.Data.DataView;
using ExportSettings = kCura.IntegrationPoints.FilesDestinationProvider.Core.ExportSettings;

namespace kCura.IntegrationPoints.Core.Services.Exporter.Images
{
	public class ImageExporterService : ExporterServiceBase
	{
		private readonly ImportSettings _settings;

		private readonly IFileRepository _fileRepository;

		public ImageExporterService(
			IDocumentRepository documentRepository, 
			IRelativityObjectManager relativityObjectManager, 
			IRepositoryFactory sourceRepositoryFactory, 
			IRepositoryFactory targetRepositoryFactory,
			IFileRepository fileRepository,
			IJobStopManager jobStopManager, 
			IHelper helper, 
			IBaseServiceContextProvider baseServiceContextProvider, 
			FieldMap[] mappedFields, 
			int startAt,
			string config, 
			int searchArtifactId, 
			ImportSettings settings)
			: base(
				documentRepository, 
				relativityObjectManager, 
				sourceRepositoryFactory, 
				targetRepositoryFactory, 
				jobStopManager, 
				helper, 
				baseServiceContextProvider, 
				mappedFields, 
				startAt,
				config, 
				searchArtifactId)
		{
			_settings = settings;
			_fileRepository = fileRepository;
		}

		public override IDataTransferContext GetDataTransferContext(IExporterTransferConfiguration transferConfiguration)
		{
			var imageTransferDataReader = new ImageTransferDataReader(this, MappedFields, BaseContext, Logger, transferConfiguration.ScratchRepositories);
			return Context ?? (Context = new ExporterTransferContext(imageTransferDataReader, transferConfiguration));
		}

		public override ArtifactDTO[] RetrieveData(int size)
		{
			Logger.LogDebug("Start retrieving data in ImageExporterService. Size: {size}, export type: {typeOfExport}, FieldArtifactIds size: {avfIdsSize}",
				size, SourceConfiguration.TypeOfExport, FieldArtifactIds.Length);

			IList<RelativityObjectSlimDto> retrievedData = DocumentRepository
				.RetrieveResultsBlockFromExportAsync(ExportJobInfo, size, RetrievedDataCount)
				.GetAwaiter().GetResult();

			Logger.LogDebug($"Retrieved {retrievedData.Count} documents in ImageExporterService");

			var imagesResult = new List<ArtifactDTO>();

			foreach (RelativityObjectSlimDto data in retrievedData)
			{
				var fields = new List<ArtifactFieldDTO>();

				int documentArtifactID = data.ArtifactID;
				if (SourceConfiguration.TypeOfExport == SourceConfiguration.ExportType.ProductionSet)
				{
					SetProducedImagesByProductionId(
						documentArtifactID, 
						fields, 
						data.FieldValues, 
						(int) ArtifactType.Document,
						imagesResult, 
						SourceConfiguration.SourceProductionId);
				}
				else
				{
					SetImagesBySavedSearch(
						documentArtifactID, 
						fields, 
						data.FieldValues, 
						(int) ArtifactType.Document,
						imagesResult);
				}

			}

			RetrievedDataCount += retrievedData.Count;

			Logger.LogDebug("Retrieved {numberOfImages} images in ImageExporterService", imagesResult.Count);
			Context.TotalItemsFound = Context.TotalItemsFound.GetValueOrDefault() + imagesResult.Count;
			return imagesResult.ToArray();
		}

		private void SetImagesBySavedSearch(
			int documentArtifactID, 
			List<ArtifactFieldDTO> fields, 
			IDictionary<string, object> fieldValues, 
			int artifactType,
			List<ArtifactDTO> imagesResult)
		{
			ExportSettings.ProductionPrecedenceType productionPrecedenceType = GetProductionPrecedenceType();
			if (productionPrecedenceType == ExportSettings.ProductionPrecedenceType.Produced)
			{
				int producedImagesCount = SetProducedImagesByPrecedence(documentArtifactID, fields, fieldValues, artifactType, imagesResult);
				if (_settings.IncludeOriginalImages && producedImagesCount == 0)
				{
					Logger.LogDebug("Produced images are not available, original images will be used. Document: {documentArtifactId}", documentArtifactID);
					SetOriginalImages(documentArtifactID, fieldValues, fields, artifactType, imagesResult);
				}
			}
			else
			{
				SetOriginalImages(documentArtifactID, fieldValues, fields, artifactType, imagesResult);
			}
		}

		private void SetOriginalImages(
			int documentArtifactID, 
			IDictionary<string, object> fieldValues, 
			List<ArtifactFieldDTO> fields, 
			int artifactType, 
			List<ArtifactDTO> result)
		{
			DataView imagesDataView = _fileRepository
				.GetImagesForDocuments(
					SourceConfiguration.SourceWorkspaceArtifactId, 
					documentIDs: new[] { documentArtifactID })
				.ToDataView();
			if (imagesDataView.Count > 0)
			{
				CreateImageArtifactDtos(imagesDataView, documentArtifactID, fields, fieldValues, artifactType, result);
			}
		}

		private int SetProducedImagesByPrecedence(
			int documentArtifactID, 
			List<ArtifactFieldDTO> fields, 
			IDictionary<string, object> fieldValues, 
			int artifactType, 
			List<ArtifactDTO> result)
		{
			foreach (ProductionDTO prod in _settings.ImagePrecedence)
			{
				int productionArtifactId = Convert.ToInt32(prod.ArtifactID);
				int producedImagesCount = SetProducedImagesByProductionId(
					documentArtifactID, 
					fields, 
					fieldValues, 
					artifactType,
					result, 
					productionArtifactId);
				if (producedImagesCount > 0)
				{
					return producedImagesCount;
				}
			}
			return 0;
		}

		private int SetProducedImagesByProductionId(
			int documentArtifactID, 
			List<ArtifactFieldDTO> fields, 
			IDictionary<string, object> fieldValues, 
			int artifactType,
			List<ArtifactDTO> result, 
			int productionArtifactId)
		{
			List<string> producedImagesDataView = _fileRepository
				.GetImagesLocationForProductionDocuments(
					SourceConfiguration.SourceWorkspaceArtifactId,
					productionArtifactId, 
					documentIDs: new[] { documentArtifactID })
				.ToDataView();
			if (producedImagesDataView.Count > 0)
			{
				CreateImageArtifactDtos(producedImagesDataView, documentArtifactID, fields, fieldValues, artifactType, result);
			}
			return producedImagesDataView.Count;
		}

		private void CreateImageArtifactDtos(
			DataView dataView, 
			int documentArtifactID, 
			List<ArtifactFieldDTO> fields, 
			IDictionary<string, object> fieldValues, 
			int artifactType,
			List<ArtifactDTO> result)
		{
			SetupBaseFields(fieldValues, fields);

			// the assumption is based on the following facts:
			// - for images we only allow maping identifier field, so _avfIds has only one object, this is guarded by validation
			// - Core Export API's RetrieveResults() method returns results based on _avfIds and in the same order (potentially adding additional columns at the end)
			string documentIdentifier = fieldValues.Values.First().ToString();

			foreach (var locationRow in dataView)
			{
				ArtifactDTO artifactDto = CreateImageArtifactDto(dataView.Table.Rows[index], documentArtifactId, documentIdentifier, fields, artifactType);
				result.Add(artifactDto);
			}
		}

		private void SetupBaseFields(IDictionary<string, object> fieldValues, List<ArtifactFieldDTO> fields)
		{
			IEnumerable<ArtifactFieldDTO> baseFields = FieldArtifactIds
				.Zip(fieldValues, (fieldArtifactID, fieldValue) => new
				{
					ArtifactID = fieldArtifactID,
					Name = fieldValue.Key,
					Value = fieldValue.Value
				})
				.Select(artifact => new ArtifactFieldDTO
				{
					Name = artifact.Name,
					ArtifactId = artifact.ArtifactID,
					Value = artifact.Value,
					FieldType = QueryFieldLookupRepository.GetFieldTypeByArtifactId(artifact.ArtifactID)
				});

			fields.AddRange(baseFields);
		}

		private ArtifactDTO CreateImageArtifactDto(
			DataRow imageDataRow, 
			int documentArtifactID, 
			string documentIdentifier,
			List<ArtifactFieldDTO> fields, int artifactType)
		{
			string fileLocation = imageDataRow[ImageLocationColumn].ToString();
			List<ArtifactFieldDTO> artifactFieldDtos = AddImageFields(fields, fileLocation, documentIdentifier);
			var artifactDto = new ArtifactDTO(documentArtifactID, artifactType, string.Empty, artifactFieldDtos);
			return artifactDto;
		}

		private List<ArtifactFieldDTO> AddImageFields(List<ArtifactFieldDTO> fields, string fileLocation, string documentIdentifier)
		{
			var fileLocationField = new ArtifactFieldDTO
			{
				Name = IntegrationPoints.Domain.Constants.SPECIAL_NATIVE_FILE_LOCATION_FIELD_NAME,
				Value = fileLocation
			};
			var nativeFileNameField = new ArtifactFieldDTO
			{
				Name = IntegrationPoints.Domain.Constants.SPECIAL_FILE_NAME_FIELD_NAME,
				Value = documentIdentifier
			};

			List<ArtifactFieldDTO> artifactFieldDtos = fields.ToList();
			artifactFieldDtos.Add(fileLocationField);
			artifactFieldDtos.Add(nativeFileNameField);
			return artifactFieldDtos;
		}

		private ExportSettings.ProductionPrecedenceType GetProductionPrecedenceType()
		{
			try
			{
				string productionPrecedence = _settings.ProductionPrecedence;
				if (string.IsNullOrEmpty(productionPrecedence))
				{
					return ExportSettings.ProductionPrecedenceType.Original;
				}
				return (ExportSettings.ProductionPrecedenceType)Enum.Parse(typeof(ExportSettings.ProductionPrecedenceType), productionPrecedence);
			}
			catch (Exception)
			{
				return ExportSettings.ProductionPrecedenceType.Original;
			}
		}
	}
}
