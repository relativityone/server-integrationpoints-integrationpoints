using System;
using System.Collections.Generic;
using System.Security.Claims;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services.Exporter.TransferContext;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Readers;
using kCura.Relativity.Client;
using Relativity.API;
using Relativity.Core.Api.Shared.Manager.Export;

namespace kCura.IntegrationPoints.Core.Services.Exporter
{
	public class RelativityExporterService : ExporterServiceBase
	{
		private readonly IFolderPathReader _folderPathReader;

		public RelativityExporterService(IRepositoryFactory sourceRepositoryFactory, IRepositoryFactory targetRepositoryFactory, IJobStopManager jobStopManager, IHelper helper,
			IFolderPathReader folderPathReader, ClaimsPrincipal claimsPrincipal, FieldMap[] mappedFields, int startAt, string config, int savedSearchArtifactId)
			: base(sourceRepositoryFactory, targetRepositoryFactory, jobStopManager, helper, claimsPrincipal, mappedFields, startAt, config, savedSearchArtifactId)
		{
			_folderPathReader = folderPathReader;
		}

		internal RelativityExporterService(IExporter exporter, IILongTextStreamFactory longTextStreamFactory, IJobStopManager jobStopManager, IHelper helper,
			IFolderPathReader folderPathReader, FieldMap[] mappedFields, HashSet<int> longTextField, int[] avfIds)
			: base(exporter, longTextStreamFactory, jobStopManager, helper, mappedFields, longTextField, avfIds)
		{
			_folderPathReader = folderPathReader;
		}

		public override IDataTransferContext GetDataTransferContext(IExporterTransferConfiguration transferConfiguration)
		{
			var documentTransferDataReader = new DocumentTransferDataReader(this, _mappedFields, _baseContext, transferConfiguration.ScratchRepositories,
				transferConfiguration.ImportSettings.FolderPathDynamic);
			var exporterTransferContext = new ExporterTransferContext(documentTransferDataReader, transferConfiguration) {TotalItemsFound = TotalRecordsFound};
			return _context ?? (_context = exporterTransferContext);
		}

		public override ArtifactDTO[] RetrieveData(int size)
		{
			List<ArtifactDTO> result = new List<ArtifactDTO>(size);
			object[] retrievedData = _exporter.RetrieveResults(_exportJobInfo.RunId, _avfIds, size);
			if (retrievedData != null)
			{
				int artifactType = (int) ArtifactType.Document;
				foreach (object data in retrievedData)
				{
					List<ArtifactFieldDTO> fields = new List<ArtifactFieldDTO>(_avfIds.Length);

					object[] fieldsValue = (object[]) data;
					int documentArtifactId = Convert.ToInt32(fieldsValue[_avfIds.Length]);

					for (int index = 0; index < _avfIds.Length; index++)
					{
						int artifactId = _fieldArtifactIds[index];
						object value = fieldsValue[index];

						Exception exception = null;
						try
						{
							if (_multipleObjectFieldArtifactIds.Contains(artifactId))
							{
								value = ExportApiDataHelper.SanitizeMultiObjectField(value);
							}
							else if (_singleChoiceFieldsArtifactIds.Contains(artifactId))
							{
								value = ExportApiDataHelper.SanitizeSingleChoiceField(value);
							}
							// export api will return a string constant represent the state of the string of which is too big. We will have to go and read this our self.
							else if (_longTextFieldArtifactIds.Contains(artifactId)
									&& global::Relativity.Constants.LONG_TEXT_EXCEEDS_MAX_LENGTH_FOR_LIST_TOKEN.Equals(value))
							{
								value = ExportApiDataHelper.RetrieveLongTextFieldAsync(_longTextStreamFactory, documentArtifactId, artifactId)
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
							Name = _exportJobInfo.ColumnNames[index],
							ArtifactId = artifactId,
							Value = value
						});
					}

					// TODO: replace String.empty
					result.Add(new ArtifactDTO(documentArtifactId, artifactType, string.Empty, fields));
				}
			}

			_folderPathReader.SetFolderPaths(result);

			_retrievedDataCount += result.Count;
			return result.ToArray();
		}
	}
}