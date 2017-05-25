using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime;
using System.Security.Claims;
using System.Text.RegularExpressions;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services.Exporter.TransferContext;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Readers;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Newtonsoft.Json;
using Relativity;
using Relativity.API;
using Relativity.Core;
using Relativity.Core.Service;
using Relativity.Core.Api.Shared.Manager.Export;
using Relativity.Data;
using Relativity.Toggles;
using ArtifactType = kCura.Relativity.Client.ArtifactType;
using ExportSettings = kCura.IntegrationPoints.FilesDestinationProvider.Core.ExportSettings;
using FileQuery = Relativity.Core.Service.FileQuery;
using QueryFieldLookup = Relativity.Core.QueryFieldLookup;
using UserPermissionsMatrix = Relativity.Core.UserPermissionsMatrix;

namespace kCura.IntegrationPoints.Core.Services.Exporter
{
	public class ImageExporterService : ExporterServiceBase
	{
		private readonly ImportSettings _settings;
		private const string ImageNameColumn = "Identifier";
		private const string ProductionNameColumn = "NativeIdentifier";
		private const string ImageLocationColumn = "Location";



		public ImageExporterService(IExporter exporter, IRepositoryFactory sourceRepositoryFactory, IRepositoryFactory targetRepositoryFactory,
			IJobStopManager jobStopManager, IHelper helper, ClaimsPrincipal claimsPrincipal, FieldMap[] mappedFields, int startAt,
			string config, int searchArtifactId, ImportSettings settings)
			: base(
				exporter, sourceRepositoryFactory, targetRepositoryFactory, jobStopManager, helper, claimsPrincipal, mappedFields, startAt,
				config, searchArtifactId)
		{
			_settings = settings;
		}

		public ImageExporterService(FieldMap[] mappedFields, IJobStopManager jobStopManager, IHelper helper) : base(mappedFields, jobStopManager, helper)
		{
		}

		public override IDataTransferContext GetDataTransferContext(IExporterTransferConfiguration transferConfiguration)
		{
			var imageTransferDataReader = new ImageTransferDataReader(this, _mappedFields, _baseContext, transferConfiguration.ScratchRepositories);
			return _context ?? (_context = new ExporterTransferContext(imageTransferDataReader, transferConfiguration));
		}

		public override ArtifactDTO[] RetrieveData(int size)
		{
			List<ArtifactDTO> imagesResult = new List<ArtifactDTO>();
			object[] retrievedData = _exporter.RetrieveResults(_exportJobInfo.RunId, _avfIds, size);

			if (retrievedData != null)
			{
				int artifactType = (int)ArtifactType.Document;
				foreach (object data in retrievedData)
				{
					List<ArtifactFieldDTO> fields = new List<ArtifactFieldDTO>();
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
			}

			_retrievedDataCount += imagesResult.Count;
			_context.TotalItemsFound = _retrievedDataCount;
			return imagesResult.ToArray();
		}

	    private void SetImagesBySavedSearch(int documentArtifactId, List<ArtifactFieldDTO> fields, object[] fieldsValue, int artifactType,
	        List<ArtifactDTO> imagesResult)
	    {
	        var productionPrecedenceType = GetProductionPrecedenceType();
	        if (productionPrecedenceType == ExportSettings.ProductionPrecedenceType.Produced)
	        {
	            var producedImagesCount = SetProducedImagesByPrecedence(documentArtifactId, fields, fieldsValue, artifactType,
	                imagesResult);
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
			kCura.Data.DataView imagesDataView = FileQuery.RetrieveAllImagesForDocuments(_baseContext, new[] { documentArtifactId });
			if (imagesDataView.Count > 0)
			{
				SetupBaseFields(documentArtifactId, fieldsValue, fields);
				for (int index = 0; index < imagesDataView.Table.Rows.Count; index++)
				{
					var artifactDto = CreateImageArtifactDto(imagesDataView.Table.Rows[index], ImageNameColumn,
						documentArtifactId, fields, artifactType, index);
					result.Add(artifactDto);
				}
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
			var producedImages = FileQuery.RetrieveImagesByProductionArtifactIDForProductionExportByDocumentSet(_baseContext,
				productionArtifactId,
				new[] {documentArtifactId});

			if (producedImages.Count > 0)
			{
				SetupBaseFields(documentArtifactId, fieldsValue, fields);
				for (int index = 0; index < producedImages.Table.Rows.Count; index++)
				{
					var artifactDto = CreateImageArtifactDto(producedImages.Table.Rows[index], ProductionNameColumn,
						documentArtifactId, fields, artifactType, index);
					result.Add(artifactDto);
				
				}
				return producedImages.Count;
			}
			return 0;
		}

		private ArtifactDTO CreateImageArtifactDto(DataRow imageDataRow, string imageNameColumn, int documentArtifactId,
			List<ArtifactFieldDTO> fields, int artifactType, int index)
		{
			string fileLocation = (string) imageDataRow[ImageLocationColumn];
			string imageFileName = UnwrapDocumentIdentifierFieldName((string) imageDataRow[imageNameColumn], index);

			var artifactFieldDtos = AddImageFields(fields, fileLocation, imageFileName);
			var artifactDto = new ArtifactDTO(documentArtifactId, artifactType, string.Empty, artifactFieldDtos);
			return artifactDto;
		}

		private List<ArtifactFieldDTO> AddImageFields(List<ArtifactFieldDTO> fields, string fileLocation, string imageFileName)
		{
			var fileLocationField = new ArtifactFieldDTO()
			{
				Name = IntegrationPoints.Domain.Constants.SPECIAL_NATIVE_FILE_LOCATION_FIELD_NAME,
				Value = fileLocation
			};
			var nativeFileNameField = new ArtifactFieldDTO()
			{
				Name = IntegrationPoints.Domain.Constants.SPECIAL_FILE_NAME_FIELD_NAME,
				Value = imageFileName
			};

			var artifactFieldDtos = fields.ToList();
			artifactFieldDtos.Add(fileLocationField);
			artifactFieldDtos.Add(nativeFileNameField);
			return artifactFieldDtos;
		}

		private string UnwrapDocumentIdentifierFieldName(string imageFileName, int rowIndex)
		{
			//Expected input format for Original Image - DocumentName_DocumentNumber_INDEX_GUID
			var imageFileNameSplitted = imageFileName?.Split('_');
			if (imageFileNameSplitted?.Length > 1)//Orignal
			{
				var documentName = imageFileNameSplitted?[0];
				var documentNamePostFix = imageFileNameSplitted?[1];
				imageFileName = $"{documentName}_{documentNamePostFix}";
				imageFileName = BuildDocumentIdentifierFieldName(imageFileName, rowIndex);
			}
			return imageFileName;
		}

		private string BuildDocumentIdentifierFieldName(string nativeFileName, int rowIndex)
		{
			return rowIndex == 0 ? nativeFileName : $"{nativeFileName}_{rowIndex:00}";
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
