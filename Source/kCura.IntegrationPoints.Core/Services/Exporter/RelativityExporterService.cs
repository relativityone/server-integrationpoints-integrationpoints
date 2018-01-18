using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services.Exporter.TransferContext;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Toggles;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Readers;
using Relativity.API;
using Relativity.Core.Api.Shared.Manager.Export;
using Relativity.Toggles;
using ArtifactType = kCura.Relativity.Client.ArtifactType;

namespace kCura.IntegrationPoints.Core.Services.Exporter
{
	public class RelativityExporterService : ExporterServiceBase
	{
		private readonly IFolderPathReader _folderPathReader;
		private readonly IToggleProvider _toggleProvider;

		public RelativityExporterService(IExporter exporter, IRepositoryFactory sourceRepositoryFactory, IRepositoryFactory targetRepositoryFactory, IJobStopManager jobStopManager, IHelper helper,
			IFolderPathReader folderPathReader, IToggleProvider toggleProvider, IBaseServiceContextProvider baseServiceContextProvider, FieldMap[] mappedFields, int startAt, string config, int searchArtifactId)
			: base(exporter, sourceRepositoryFactory, targetRepositoryFactory, jobStopManager, helper, baseServiceContextProvider, mappedFields, startAt, config, searchArtifactId)
		{
			_folderPathReader = folderPathReader;
			_toggleProvider = toggleProvider;
		}

		internal RelativityExporterService(IExporter exporter, IILongTextStreamFactory longTextStreamFactory, IJobStopManager jobStopManager, IHelper helper,
			IQueryFieldLookupRepository queryFieldLookupRepository, IFolderPathReader folderPathReader, IToggleProvider toggleProvider, FieldMap[] mappedFields, HashSet<int> longTextField, int[] avfIds)
			: base(exporter, longTextStreamFactory, jobStopManager, helper, queryFieldLookupRepository, mappedFields, longTextField, avfIds)
		{
			_folderPathReader = folderPathReader;
			_toggleProvider = toggleProvider;
		}

		public override IDataTransferContext GetDataTransferContext(IExporterTransferConfiguration transferConfiguration)
		{
			var documentTransferDataReader = new DocumentTransferDataReader(this, MappedFields, BaseContext,
				transferConfiguration.ScratchRepositories, LongTextStreamFactory,
				_toggleProvider,
				transferConfiguration.ImportSettings.UseDynamicFolderPath);
			var exporterTransferContext = new ExporterTransferContext(documentTransferDataReader, transferConfiguration) {TotalItemsFound = TotalRecordsFound};
			return Context ?? (Context = exporterTransferContext);
		}

		public override ArtifactDTO[] RetrieveData(int size)
		{
			List<ArtifactDTO> result = new List<ArtifactDTO>(size);
			object[] retrievedData = Exporter.RetrieveResults(ExportJobInfo.RunId, AvfIds, size);
			if (retrievedData != null)
			{
				int artifactType = (int)ArtifactType.Document;
				foreach (object data in retrievedData)
				{
					List<ArtifactFieldDTO> fields = new List<ArtifactFieldDTO>(AvfIds.Length);

					object[] fieldsValue = (object[]) data;
					int documentArtifactId = Convert.ToInt32(fieldsValue[AvfIds.Length]);

					SetupBaseFields(documentArtifactId, fieldsValue, fields);

					// TODO: replace String.empty
					result.Add(new ArtifactDTO(documentArtifactId, artifactType, string.Empty, fields));
				}
			}
			
			_folderPathReader.SetFolderPaths(result);
			RetrievedDataCount += result.Count;
			return result.ToArray();
		}


		private void SetupBaseFields(int documentArtifactId, object[] fieldsValue, List<ArtifactFieldDTO> fields)
		{
			for (int index = 0; index < AvfIds.Length; index++)
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
					else if (!_toggleProvider.IsEnabled<UseStreamsForBigLongTextFieldsToggle>() && LongTextFieldArtifactIds.Contains(artifactId)
																								&& global::Relativity.Constants.LONG_TEXT_EXCEEDS_MAX_LENGTH_FOR_LIST_TOKEN.Equals(value))
					{
						value = ExportApiDataHelper.RetrieveLongTextFieldAsync(LongTextStreamFactory, documentArtifactId, artifactId)
							.GetResultsWithoutContextSync();
					}
				}
				catch (Exception ex)
				{
					LogRetrievingDataError(ex);
					exception = ex;
				}

				fields.Add(new LazyExceptArtifactFieldDto(exception)
				{
					Name = ExportJobInfo.ColumnNames[index],
					ArtifactId = artifactId,
					Value = value,
					FieldType = QueryFieldLookupRepository.GetFieldByArtifactId(artifactId).FieldType.ToString()
				});
			}
		}
	}
}