using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services.Exporter.Base;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Readers;
using Relativity.API;
using Relativity.Core.Api.Shared.Manager.Export;
using ArtifactType = kCura.Relativity.Client.ArtifactType;

namespace kCura.IntegrationPoints.Core.Services.Exporter
{
	public class RelativityExporterService : ExporterServiceBase
	{
		private readonly IFolderPathReader _folderPathReader;

		public RelativityExporterService(
			IExporter exporter, 
			IRepositoryFactory sourceRepositoryFactory, 
			IRepositoryFactory targetRepositoryFactory, 
			IJobStopManager jobStopManager, 
			IHelper helper,
			IFolderPathReader folderPathReader, 
			IBaseServiceContextProvider baseServiceContextProvider, 
			FieldMap[] mappedFields, 
			int startAt, 
			string config, 
			int searchArtifactId)
			: base(
				exporter, 
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
			_folderPathReader = folderPathReader;
		}

		internal RelativityExporterService(
			IExporter exporter, 
			IRelativityObjectManager relativityObjectManager,
			IJobStopManager jobStopManager, 
			IHelper helper,
			IQueryFieldLookupRepository queryFieldLookupRepository, 
			IFolderPathReader folderPathReader, 
			FieldMap[] mappedFields, 
			HashSet<int> longTextField, 
			int[] avfIds)
			: base(exporter, relativityObjectManager, jobStopManager, helper, queryFieldLookupRepository, mappedFields, longTextField, avfIds)
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
				Logger,
				transferConfiguration.ImportSettings.UseDynamicFolderPath);
			var exporterTransferContext = 
				new ExporterTransferContext(documentTransferDataReader, transferConfiguration)
					{ TotalItemsFound = TotalRecordsFound };
			return Context ?? (Context = exporterTransferContext);
		}

		public override ArtifactDTO[] RetrieveData(int size)
		{
			Logger.LogDebug("Start retrieving data in RelativityExporterService. Size: {size}, export type: {typeOfExport}, AvfIds size: {avfIdsSize}",
				size, SourceConfiguration?.TypeOfExport, ArtifactViewFieldIds.Length);
			var result = new List<ArtifactDTO>(size);
			object[] retrievedData = Exporter.RetrieveResults(ExportJobInfo.RunId, ArtifactViewFieldIds, size);
			if (retrievedData != null)
			{
				Logger.LogDebug("Retrieved {numberOfDocuments} documents in RelativityExporterService", retrievedData.Length);
				int artifactType = (int)ArtifactType.Document;
				foreach (object data in retrievedData)
				{
					var fields = new List<ArtifactFieldDTO>(ArtifactViewFieldIds.Length);

					object[] fieldsValue = (object[])data;
					int documentArtifactId = Convert.ToInt32(fieldsValue[ArtifactViewFieldIds.Length]);

					SetupBaseFields(fieldsValue, fields);

					// TODO: replace String.empty
					result.Add(new ArtifactDTO(documentArtifactId, artifactType, string.Empty, fields));
				}
			}

			Logger.LogDebug("Before setting folder paths for documents");
			_folderPathReader.SetFolderPaths(result);
			Logger.LogDebug("After setting folder paths for documents");
			RetrievedDataCount += result.Count;
			return result.ToArray();
		}


		private void SetupBaseFields(object[] fieldsValue, List<ArtifactFieldDTO> fields)
		{
			for (int index = 0; index < ArtifactViewFieldIds.Length; index++)
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
					Name = ExportJobInfo.ColumnNames[index],
					ArtifactId = artifactId,
					Value = value,
					FieldType = QueryFieldLookupRepository.GetFieldTypeByArtifactId(artifactId)
				});
			}
		}
	}
}