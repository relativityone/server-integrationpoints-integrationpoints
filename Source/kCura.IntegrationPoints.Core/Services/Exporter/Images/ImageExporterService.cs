﻿using System;
using System.Collections.Generic;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services.Exporter.Base;
using kCura.IntegrationPoints.Data.DTO;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Readers;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.Utility.Extensions;
using kCura.WinEDDS.Service.Export;
using LanguageExt;
using Relativity.API;
using ArtifactType = kCura.Relativity.Client.ArtifactType;
using ExportSettings = kCura.IntegrationPoints.FilesDestinationProvider.Core.ExportSettings;

namespace kCura.IntegrationPoints.Core.Services.Exporter.Images
{
	public class ImageExporterService : ExporterServiceBase
	{
		private readonly ImportSettings _settings;
		private readonly Func<ISearchManager> _searchManagerFunc;
		private int _readDocumentsCounts;

		public ImageExporterService(
			IDocumentRepository documentRepository,
			IRelativityObjectManager relativityObjectManager,
			IRepositoryFactory sourceRepositoryFactory,
			IRepositoryFactory targetRepositoryFactory,
			IFileRepository fileRepository,
			IJobStopManager jobStopManager,
			IHelper helper,
			ISerializer serializer,
			FieldMap[] mappedFields,
			int startAt,
			SourceConfiguration sourceConfiguration,
			int searchArtifactId,
			ImportSettings settings, Func<ISearchManager> searchManagerFunc)
			: base(
				documentRepository,
				relativityObjectManager,
				sourceRepositoryFactory,
				targetRepositoryFactory,
				jobStopManager,
				helper,
				fileRepository,
				serializer,
				mappedFields,
				startAt,
				sourceConfiguration,
				searchArtifactId)
		{
			_settings = settings;
			_searchManagerFunc = searchManagerFunc;
			_readDocumentsCounts = 0;
		}

		public override IDataTransferContext GetDataTransferContext(IExporterTransferConfiguration transferConfiguration)
		{
			var imageTransferDataReader = new ImageTransferDataReader(this, MappedFields, Logger, transferConfiguration.ScratchRepositories);
			return Context ?? (Context = new ExporterTransferContext(imageTransferDataReader, transferConfiguration));
		}

		public override ArtifactDTO[] RetrieveData(int size)
		{
			Logger.LogInformation("Start retrieving data in ImageExporterService. Size: {size}, export type: {typeOfExport}, FieldArtifactIds size: {avfIdsSize}",
				size, SourceConfiguration.TypeOfExport, FieldArtifactIds.Length);

			Dictionary<int, RelativityObjectSlimDto> retrievedData = DocumentRepository
				.RetrieveResultsBlockFromExportAsync(ExportJobInfo, size, RetrievedDataCount)
				.GetAwaiter().GetResult()
				.ToDictionary(x => x.ArtifactID, x => x);

			Logger.LogInformation("Retrieved {count} documents in ImageExporterService [{startArtifactId} - {endArtifactId}]", retrievedData.Count, retrievedData.First().Key, retrievedData.Last().Key);
			Logger.LogDebug("Retrieved documents Ids: [ids]", string.Join(", ", retrievedData.Keys));

			var imagesResult = new List<ArtifactDTO>();

			using (ISearchManager searchManager = _searchManagerFunc())
			{


				IDictionary<int, List<string>> documentsWithImages = new Dictionary<int, List<string>>();
				List<int> documentsWithoutImages = new List<int>(retrievedData.Keys);

				if (SourceConfiguration.TypeOfExport == SourceConfiguration.ExportType.ProductionSet)
				{
					GetImagesForDocumentsInProduction(SourceConfiguration.SourceProductionId, documentsWithImages, documentsWithoutImages, searchManager);
				}
				else
				{
					ExportSettings.ProductionPrecedenceType productionPrecedenceType = GetProductionPrecedenceType();

					if (productionPrecedenceType == ExportSettings.ProductionPrecedenceType.Produced)
					{
						Logger.LogInformation("Processing production precedences: {productionPrecedences}", string.Join(", ", _settings.ImagePrecedence.Select(x => x.ArtifactID)));

						int[] productionsArtifactIds = _settings.ImagePrecedence.Select(x => Convert.ToInt32(x.ArtifactID)).ToArray();

						foreach (var productionsArtifactId in productionsArtifactIds)
						{
							GetImagesForDocumentsInProduction(productionsArtifactId, documentsWithImages, documentsWithoutImages, searchManager);
							if (documentsWithoutImages.Count == 0)
							{
								break;
							}
						}
					}

					if (_settings.IncludeOriginalImages || productionPrecedenceType == ExportSettings.ProductionPrecedenceType.Original)
					{

						ILookup<int, string> originalImageLocationsForDocuments =
							GetOriginalImages(documentsWithoutImages.ToArray(), searchManager);


						int documentsWithImagesCount = MarkDocumentsWithImages(documentsWithImages, originalImageLocationsForDocuments, documentsWithoutImages);
						Logger.LogInformation("Found {documentsWithImagesCount} images in original images",
							documentsWithImagesCount);
						Logger.LogInformation("Documents found in original images: [{documentsIds}]",
							string.Join(", ",
								originalImageLocationsForDocuments.Where(x => x.Any()).Select(x => x.Key)));
					}
				}

				if (documentsWithoutImages.Any())
				{
					Logger.LogInformation("Did not find images for some documents: {documentsIds}",
						string.Join(", ", documentsWithoutImages));
				}

				Logger.LogInformation("Creating DTOs for images");
				imagesResult.AddRange(
					documentsWithImages.SelectMany(doc =>
						doc.Value.Select(loc => CreateImageArtifactDto(
								fileLocation: loc,
								documentArtifactID: doc.Key,
								documentIdentifier: GetDocumentIdentifier(retrievedData[doc.Key]),
								fields: GetBaseFields(retrievedData[doc.Key].FieldValues).ToList(),
								artifactType: (int)ArtifactType.Document
							)
						)
					)
				);
				RetrievedDataCount += retrievedData.Count;
				_readDocumentsCounts += size;

				Logger.LogInformation("Retrieved {numberOfImages} images for {numberOfDocuments} in ImageExporterService", imagesResult.Count, documentsWithImages.Count);
				Context.TotalItemsFound = Context.TotalItemsFound.GetValueOrDefault() + imagesResult.Count;

				Logger.LogInformation("Read {readDocumentsCount} out of {totalDocumentCount}", _readDocumentsCounts, TotalRecordsFound);

				return imagesResult.ToArray();
			}
		}

		private string GetDocumentIdentifier(RelativityObjectSlimDto documentSlim)
		{
			// the assumption is based on the following facts:
			// - for images we only allow maping identifier field, so _avfIds has only one object, this is guarded by validation
			// - Core Export API's RetrieveResults() method returns results based on _avfIds and in the same order (potentially adding additional columns at the end)
			return documentSlim.FieldValues.Values.First().ToString();
		}

		private void GetImagesForDocumentsInProduction(int productionsArtifactId, IDictionary<int, List<string>> documentsWithImages, List<int> documentsWithoutImages, ISearchManager searchManager)
		{
			Logger.LogInformation("Getting images for production: {productionsArtifactId}", productionsArtifactId);

			ILookup<int, string> imageLocationsForDocuments = GetImageLocationsForDocumentsFromProduction(
				documentsWithoutImages.ToArray(),
				productionsArtifactId,
				searchManager);


			int documentsWithImagesCount = MarkDocumentsWithImages(documentsWithImages, imageLocationsForDocuments, documentsWithoutImages);

			Logger.LogInformation("Found {documentsWithImagesCount} images in production {productionArtifactId}",
				documentsWithImagesCount, productionsArtifactId);
			Logger.LogInformation("Documents found in production {productionId}: [{documentsIds}]", productionsArtifactId,
				string.Join(", ", imageLocationsForDocuments.Where(x => x.Any()).Select(x => x.Key)));
		}


		private IEnumerable<ArtifactFieldDTO> GetBaseFields(IDictionary<string, object> fieldValues)
		{
			return FieldArtifactIds
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
					FieldType = QueryFieldLookupRepository.GetFieldTypeByArtifactID(artifact.ArtifactID)
				});
		}

		private static int MarkDocumentsWithImages(IDictionary<int, List<string>> documentsWithImages, ILookup<int, string> documentsToMark,
			List<int> documentsWithoutImages)
		{
			int i = 0;
			foreach (var doc in documentsToMark.Where(x => x.Any()))
			{
				if (!documentsWithImages.ContainsKey(doc.Key))
					documentsWithImages.Add(doc.Key, doc.ToList());
				documentsWithoutImages.Remove(doc.Key);
				i++;
			}

			return i;
		}

		private ILookup<int, string> GetImageLocationsForDocumentsFromProduction(int[] documentArtifactIds, int productionsArtifactId, ISearchManager searchManager)
		{
			return FileRepository
				.GetImagesLocationForProductionDocuments(
					SourceConfiguration.SourceWorkspaceArtifactId,
					productionsArtifactId,
					documentArtifactIds,
					searchManager);
		}



		private ILookup<int, string> GetOriginalImages(
			int[] documentArtifactIds,
			ISearchManager searchManager)
		{
			ILookup<int, string> imagesDataView = FileRepository
				.GetImagesLocationForDocuments(
					SourceConfiguration.SourceWorkspaceArtifactId,
					documentArtifactIds,
					searchManager);

			return imagesDataView;

		}

		private ArtifactDTO CreateImageArtifactDto(
			string fileLocation,
			int documentArtifactID,
			string documentIdentifier,
			List<ArtifactFieldDTO> fields, int artifactType)
		{
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
