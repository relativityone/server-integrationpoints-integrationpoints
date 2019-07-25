using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services.Exporter.Base;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data.DTO;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Readers;
using Relativity.API;
using ArtifactType = kCura.Relativity.Client.ArtifactType;

namespace kCura.IntegrationPoints.Core.Services.Exporter
{
	public class RelativityExporterService : ExporterServiceBase
	{
		private readonly IFolderPathReader _folderPathReader;

		public RelativityExporterService(
			IDocumentRepository documentRepository, 
			IRelativityObjectManager relativityObjectManager,
			IRepositoryFactory sourceRepositoryFactory, 
			IRepositoryFactory targetRepositoryFactory, 
			IJobStopManager jobStopManager, 
			IHelper helper,
			IFolderPathReader folderPathReader, 
			IBaseServiceContextProvider baseServiceContextProvider,
			IFileRepository fileRepository,
			FieldMap[] mappedFields, 
			int startAt, 
			string config, 
			int searchArtifactId)
			: base(
				documentRepository,
				relativityObjectManager,
				sourceRepositoryFactory, 
				targetRepositoryFactory, 
				jobStopManager, 
				helper, 
				baseServiceContextProvider,
				fileRepository,
				mappedFields, 
				startAt, 
				config, 
				searchArtifactId)
		{
			_folderPathReader = folderPathReader;
		}

		public override IDataTransferContext GetDataTransferContext(IExporterTransferConfiguration transferConfiguration)
		{
			var documentTransferDataReader = new DocumentTransferDataReader(
				this, 
				MappedFields, 
				BaseContext,
				transferConfiguration.ScratchRepositories, 
				RelativityObjectManager,
				DocumentRepository,
				Logger,
				QueryFieldLookupRepository,
				FileRepository,
				transferConfiguration.ImportSettings.UseDynamicFolderPath,
				SourceConfiguration.SourceWorkspaceArtifactId);
			var exporterTransferContext = 
				new ExporterTransferContext(documentTransferDataReader, transferConfiguration)
					{ TotalItemsFound = TotalRecordsFound };
			return Context ?? (Context = exporterTransferContext);
		}

		public override ArtifactDTO[] RetrieveData(int size)
		{
			Logger.LogDebug("Start retrieving data in RelativityExporterService. Size: {size}, export type: {typeOfExport}, FieldArtifactIds size: {avfIdsSize}",
				size, SourceConfiguration?.TypeOfExport, FieldArtifactIds.Length);

			IList<RelativityObjectSlimDto> retrievedData = DocumentRepository
					.RetrieveResultsBlockFromExportAsync(ExportJobInfo, size, RetrievedDataCount)
					.GetAwaiter().GetResult();

			Logger.LogDebug($"Retrieved {retrievedData.Count} documents in ImageExporterService");

			var result = new List<ArtifactDTO>(size);

			foreach (RelativityObjectSlimDto data in retrievedData)
			{
				var fields = new List<ArtifactFieldDTO>(FieldArtifactIds.Length);

				int documentArtifactID = data.ArtifactID;

				SetupBaseFields(data.FieldValues.Values.ToArray(), fields);

				// TODO: replace String.empty
				string textIdentifier = string.Empty;
				result.Add(new ArtifactDTO(documentArtifactID, (int) ArtifactType.Document, textIdentifier, fields));
			}

			Logger.LogDebug("Before setting folder paths for documents");
			_folderPathReader.SetFolderPaths(result);
			Logger.LogDebug("After setting folder paths for documents");
			RetrievedDataCount += result.Count;
			return result.ToArray();
		}


		private void SetupBaseFields(object[] fieldsValue, List<ArtifactFieldDTO> fields)
		{
			for (int index = 0; index < FieldArtifactIds.Length; index++)
			{
				int artifactId = FieldArtifactIds[index];
				object value = fieldsValue[index];

				Exception exception = null;
				try
				{
					if (MultipleObjectFieldArtifactIds.Contains(artifactId))
					{
						value = ExportApiDataHelper.SanitizeMultiObjectField(value);
					}
					else if (SingleChoiceFieldsArtifactIds.Contains(artifactId))
					{
						value = ExportApiDataHelper.SanitizeSingleChoiceField(value);
					}
				}
				catch (Exception ex)
				{
					LogRetrievingDataError(ex);
					exception = new IntegrationPointsException($"Error occured while sanitizing  field value in {nameof(RelativityExporterService)}", ex);
				}

				fields.Add(new LazyExceptArtifactFieldDto(exception)
				{
					Name = ExportJobInfo.FieldNames[index],
					ArtifactId = artifactId,
					Value = value,
					FieldType = QueryFieldLookupRepository.GetFieldTypeByArtifactId(artifactId)
				});
			}
		}
	}
}