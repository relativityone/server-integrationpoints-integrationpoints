using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services.Exporter.TransferContext;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Readers;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Relativity.API;
using Relativity.Core.Api.Shared.Manager.Export;
using ArtifactType = kCura.Relativity.Client.ArtifactType;
using ExportSettings = kCura.IntegrationPoints.FilesDestinationProvider.Core.ExportSettings;
using FileQuery = Relativity.Core.Service.FileQuery;

namespace kCura.IntegrationPoints.Core.Services.Exporter
{
	public class ImageExporterService : ExporterServiceBase
	{
		private readonly ImportSettings _settings;
		private const string ImageLocationColumn = "Location";

		private readonly IFileRepository _fileRepository;

		public ImageExporterService(IExporter exporter, IRepositoryFactory sourceRepositoryFactory, IRepositoryFactory targetRepositoryFactory,
			IJobStopManager jobStopManager, IHelper helper, ClaimsPrincipal claimsPrincipal, FieldMap[] mappedFields, int startAt,
			string config, int searchArtifactId, ImportSettings settings)
			: base(
				exporter, sourceRepositoryFactory, targetRepositoryFactory, jobStopManager, helper, claimsPrincipal, mappedFields, startAt,
				config, searchArtifactId)
		{
			_settings = settings;
			_fileRepository = sourceRepositoryFactory.GetFileRepository(_sourceConfiguration.SourceWorkspaceArtifactId);
		}

		public override IDataTransferContext GetDataTransferContext(IExporterTransferConfiguration transferConfiguration)
		{
			var imageTransferDataReader = new ImageTransferDataReader(this, _mappedFields, _baseContext, transferConfiguration.ScratchRepositories);
			return _context ?? (_context = new ExporterTransferContext(imageTransferDataReader, transferConfiguration));
		}

		public override ArtifactDTO[] RetrieveData(int size)
		{
			var imagesResult = new List<ArtifactDTO>();
			object[] retrievedData = _exporter.RetrieveResults(_exportJobInfo.RunId, _avfIds, size);

			if (retrievedData != null)
			{
				int artifactType = (int)ArtifactType.Document;
				foreach (object data in retrievedData)
				{
					var fields = new List<ArtifactFieldDTO>();
					object[] fieldsValue = (object[])data;

					int documentArtifactId = Convert.ToInt32(fieldsValue[_avfIds.Length]);
					if (_sourceConfiguration.TypeOfExport == SourceConfiguration.ExportType.ProductionSet)
					{
						SetProducedImagesByProductionId(documentArtifactId, fields, fieldsValue, artifactType, imagesResult,
							_sourceConfiguration.SourceProductionId);
					}
					else
					{
					    SetImagesBySavedSearch(documentArtifactId, fields, fieldsValue, artifactType, imagesResult);
					}
					
				}
				_retrievedDataCount += retrievedData.Length;
			}
			
			_context.TotalItemsFound = _context.TotalItemsFound.GetValueOrDefault() + imagesResult.Count;
			return imagesResult.ToArray();
		}

	    private void SetImagesBySavedSearch(int documentArtifactId, List<ArtifactFieldDTO> fields, object[] fieldsValue, int artifactType,
	        List<ArtifactDTO> imagesResult)
	    {
	        var productionPrecedenceType = GetProductionPrecedenceType();
	        if (productionPrecedenceType == ExportSettings.ProductionPrecedenceType.Produced)
	        {
	            var producedImagesCount = SetProducedImagesByPrecedence(documentArtifactId, fields, fieldsValue, artifactType, imagesResult);
	            if (_settings.IncludeOriginalImages && producedImagesCount == 0)
	            {
	                SetOriginalImages(documentArtifactId, fieldsValue, fields, artifactType, imagesResult);
	            }
	        }
	        else
	        {
	            SetOriginalImages(documentArtifactId, fieldsValue, fields, artifactType, imagesResult);
	        }
	    }

	    private void SetOriginalImages(int documentArtifactId, object[] fieldsValue, List<ArtifactFieldDTO> fields, int artifactType, List<ArtifactDTO> result)
		{
			kCura.Data.DataView imagesDataView = _fileRepository.RetrieveAllImagesForDocuments(documentArtifactId);
			if (imagesDataView.Count > 0)
			{
				CreateImageArtifactDtos(imagesDataView, documentArtifactId, fields, fieldsValue, artifactType, result);
			}
		}

		private int SetProducedImagesByPrecedence(int documentArtifactId, List<ArtifactFieldDTO> fields, object[] fieldsValue, int artifactType, List<ArtifactDTO> result)
		{
			foreach (var prod in _settings.ImagePrecedence)
			{
				var productionArtifactId = Convert.ToInt32(prod.ArtifactID);
				var producedImagesCount = SetProducedImagesByProductionId(documentArtifactId, fields, fieldsValue, artifactType, result, productionArtifactId);
				if (producedImagesCount > 0)
				{
					return producedImagesCount;
				}
			}
			return 0;
		}

		private int SetProducedImagesByProductionId(int documentArtifactId, List<ArtifactFieldDTO> fields, object[] fieldsValue, int artifactType,
			List<ArtifactDTO> result, int productionArtifactId)
		{
			var producedImages = _fileRepository.RetrieveImagesByProductionArtifactIDForProductionExportByDocumentSet(productionArtifactId, documentArtifactId);
			if (producedImages.Count > 0)
			{
				CreateImageArtifactDtos(producedImages, documentArtifactId, fields, fieldsValue, artifactType, result);
				return producedImages.Count;
			}
			return 0;
		}

		private void CreateImageArtifactDtos(kCura.Data.DataView dataView, int documentArtifactId, List<ArtifactFieldDTO> fields, object[] fieldsValue, int artifactType,
			List<ArtifactDTO> result)
		{
			SetupBaseFields(documentArtifactId, fieldsValue, fields);

			// the assumption is based on the following facts:
			// - for images we only allow maping identifier field, so _avfIds has only one object, this is guarded by validation
			// - Core Export API's RetrieveResults() method returns results based on _avfIds and in the same order (potentially adding additional columns at the end)
			string documentIdentifier = fieldsValue[0].ToString(); 

			for (int index = 0; index < dataView.Table.Rows.Count; index++)
			{
				var artifactDto = CreateImageArtifactDto(dataView.Table.Rows[index], documentArtifactId,
					documentIdentifier, fields, artifactType);
				result.Add(artifactDto);
			}
		}

		private ArtifactDTO CreateImageArtifactDto(DataRow imageDataRow, int documentArtifactId, string documentIdentifier,
			List<ArtifactFieldDTO> fields, int artifactType)
		{
			string fileLocation = (string) imageDataRow[ImageLocationColumn];
			var artifactFieldDtos = AddImageFields(fields, fileLocation, documentIdentifier);
			var artifactDto = new ArtifactDTO(documentArtifactId, artifactType, string.Empty, artifactFieldDtos);
			return artifactDto;
		}

		private List<ArtifactFieldDTO> AddImageFields(List<ArtifactFieldDTO> fields, string fileLocation, string documentIdentifier)
		{
			var fileLocationField = new ArtifactFieldDTO()
			{
				Name = IntegrationPoints.Domain.Constants.SPECIAL_NATIVE_FILE_LOCATION_FIELD_NAME,
				Value = fileLocation
			};
			var nativeFileNameField = new ArtifactFieldDTO()
			{
				Name = IntegrationPoints.Domain.Constants.SPECIAL_FILE_NAME_FIELD_NAME,
				Value = documentIdentifier
			};

			var artifactFieldDtos = fields.ToList();
			artifactFieldDtos.Add(fileLocationField);
			artifactFieldDtos.Add(nativeFileNameField);
			return artifactFieldDtos;
		}

		private ExportSettings.ProductionPrecedenceType GetProductionPrecedenceType()
		{
			try
			{
				var productionPrecedence = _settings.ProductionPrecedence;
				if (String.IsNullOrEmpty(productionPrecedence))
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
