using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using kCura.IntegrationPoints.Common;
using kCura.IntegrationPoints.Common.Constants;
using kCura.IntegrationPoints.Common.Handlers;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;
using kCura.IntegrationPoints.Data.DTO;
using kCura.IntegrationPoints.Data.Repositories.DTO;
using kCura.WinEDDS.Service.Export;
using Relativity.API;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class FileRepository : IFileRepository
	{
		private const ushort _MAX_NUMBER_OF_RETRIES = 3;
		private const ushort _EXPONENTIAL_WAIT_TIME_BASE_IN_SEC = 3;

		private const string _DOCUMENT_ARTIFACT_ID_COLUMN = "DocumentArtifactID";
		private const string _FILE_NAME_COLUMN = "Filename";
		private const string _LOCATION_COLUMN = "Location";
		private const string _FILE_SIZE_COLUMN = "Size";

		private const string _DOCUMENT_ARTIFACT_ID_COLUMN_NAME = "DocumentArtifactID";
		private const string _LOCATION_COLUMN_NAME = "Location";
		private const string _FILENAME_COLUMN_NAME_PRODUCTION = "ImageFileName";
		private const string _SIZE_COLUMN_NAME_PRODUCTION = "ImageSize";
		private const string _NATIVE_IDENTIFIER = "NativeIdentifier";

		private const string _FILENAME_COLUMN_NAME = "Filename";
		private const string _SIZE_COLUMN_NAME = "Size";

		private readonly IServicesMgr _servicesMgr;
		private readonly IExternalServiceInstrumentationProvider _instrumentationProvider;
		private readonly IRetryHandler _retryHandler;

		public FileRepository(IServicesMgr servicesMgr, IExternalServiceInstrumentationProvider instrumentationProvider, IRetryHandlerFactory retryHandlerFactory)
		{
			_servicesMgr = servicesMgr;
			_retryHandler = retryHandlerFactory.Create(_MAX_NUMBER_OF_RETRIES, _EXPONENTIAL_WAIT_TIME_BASE_IN_SEC);
			_instrumentationProvider = instrumentationProvider;
		}

		public ILookup<int, ImageFile> GetImagesLocationForProductionDocuments(int workspaceId, int productionId, int[] documentIDs)
		{
			ThrowWhenNullArgument(documentIDs, nameof(documentIDs));

			if (!documentIDs.Any())
			{
				return Enumerable.Empty<int>().ToLookup(x => default(int), x => default(ImageFile));
			}

			IExternalServiceSimpleInstrumentation instrumentation = CreateInstrumentation(operationName: nameof(ISearchManager.RetrieveImagesForProductionDocuments));

			ILookup<int, ImageFile> fileLocations = ToProducedImageFilesLookup(
				_retryHandler.ExecuteWithRetries(
					() => instrumentation.Execute(
						() =>
						{
							using (ISearchService searchService = _servicesMgr.CreateProxy<ISearchService>(ExecutionIdentity.CurrentUser))
							{
								DataSetWrapper dataSetWrapper = searchService
									.RetrieveImagesByProductionArtifactIDForProductionExportByDocumentSetAsync(workspaceId, productionId, documentIDs, string.Empty)
									.GetAwaiter()
									.GetResult();
								return dataSetWrapper.Unwrap();
							}
						})
				), productionId);

			return fileLocations;
		}

		public ILookup<int, ImageFile> GetImagesLocationForDocuments(int workspaceID, int[] documentIDs)
		{
			ThrowWhenNullArgument(documentIDs, nameof(documentIDs));

			if (!documentIDs.Any())
			{
				return Enumerable.Empty<int>().ToLookup(x => default(int), x => default(ImageFile));
			}

			IExternalServiceSimpleInstrumentation instrumentation = CreateInstrumentation(operationName: nameof(ISearchManager.RetrieveImagesForDocuments));

			ILookup<int, ImageFile> fileLocations = ToImageFilesLookup(
				_retryHandler.ExecuteWithRetries(
					() => instrumentation.Execute(
						() =>
						{
							using (ISearchService searchService = _servicesMgr.CreateProxy<ISearchService>(ExecutionIdentity.CurrentUser))
							{
								DataSetWrapper dataSetWrapper = searchService
									.RetrieveImagesForSearchAsync(workspaceID, documentIDs, string.Empty)
									.GetAwaiter()
									.GetResult();
								return dataSetWrapper.Unwrap();
							}
						})
				));

			return fileLocations;
		}

		public List<FileDto> GetNativesForDocuments(int workspaceID, int[] documentIDs)
		{
			ThrowWhenNullArgument(documentIDs, nameof(documentIDs));

			if (!documentIDs.Any())
			{
				return new List<FileDto>();
			}

			string documentIDsString = string.Join(",", documentIDs.Select(x => x.ToString()));

			IExternalServiceSimpleInstrumentation instrumentation = CreateInstrumentation(operationName: nameof(ISearchManager.RetrieveNativesForSearch));

			List<FileDto> files = ToFileDtoList(
				_retryHandler.ExecuteWithRetries(
					() => instrumentation.Execute(
						() =>
						{
							using (ISearchService searchService = _servicesMgr.CreateProxy<ISearchService>(ExecutionIdentity.CurrentUser))
							{
								DataSetWrapper dataSetWrapper = searchService
									.RetrieveNativesForSearchAsync(workspaceID, documentIDsString, string.Empty)
									.GetAwaiter()
									.GetResult();
								return dataSetWrapper.Unwrap();
							}
						})
				));

			return files;
		}

		private ILookup<int, ImageFile> ToProducedImageFilesLookup(DataSet imagesFromProductionDataSet, int productionId)
		{
			return imagesFromProductionDataSet.Tables[0].AsEnumerable()
				.ToLookup(x => (int)x[_DOCUMENT_ARTIFACT_ID_COLUMN_NAME], x => GetImageFileFromProduction(x, productionId));
		}

		private ImageFile GetImageFileFromProduction(DataRow dataRow, int productionId)
		{
			int documentArtifactId = GetValue<int>(dataRow, _DOCUMENT_ARTIFACT_ID_COLUMN_NAME);
			string location = GetValue<string>(dataRow, _LOCATION_COLUMN_NAME);
			string fileName = GetValue<string>(dataRow, _FILENAME_COLUMN_NAME_PRODUCTION);
			long size = GetValue<long>(dataRow, _SIZE_COLUMN_NAME_PRODUCTION);
			string nativeIdentifier = GetValue<string>(dataRow, _NATIVE_IDENTIFIER);

			return new ImageFile(documentArtifactId, location, fileName, size, productionId, nativeIdentifier);
		}

		private ImageFile GetImageFile(DataRow dataRow)
		{
			int documentArtifactId = GetValue<int>(dataRow, _DOCUMENT_ARTIFACT_ID_COLUMN_NAME);
			string location = GetValue<string>(dataRow, _LOCATION_COLUMN_NAME);
			string fileName = GetValue<string>(dataRow, _FILENAME_COLUMN_NAME);
			long size = GetValue<long>(dataRow, _SIZE_COLUMN_NAME);

			return new ImageFile(documentArtifactId, location, fileName, size);
		}

		private T GetValue<T>(DataRow row, string columnName)
		{
			object value = null;
			try
			{
				value = row[columnName];
				return (T)Convert.ChangeType(value, typeof(T));
			}
			catch (Exception ex)
			{
				throw new FileRepositoryException($"Error while retrieving image file info from column \"{columnName}\". Value: \"{value}\" Requested type: \"{typeof(T)}\"", ex);
			}
		}

		private ILookup<int, ImageFile> ToImageFilesLookup(DataSet dataSet)
		{
			return dataSet.Tables[0].AsEnumerable()
				.ToLookup(x => (int)x[_DOCUMENT_ARTIFACT_ID_COLUMN_NAME], GetImageFile);
		}

		private void ThrowWhenNullArgument<T>(T argument, string argumentName)
		{
			if (argument == null)
			{
				throw new ArgumentNullException(argumentName);
			}
		}

		private IExternalServiceSimpleInstrumentation CreateInstrumentation(string operationName)
		{
			return _instrumentationProvider.CreateSimple(
				ExternalServiceTypes.KEPLER,
				nameof(ISearchManager),
				operationName
			);
		}

		private static List<FileDto> ToFileDtoList(DataSet nativeFileDataSet)
		{
			IEnumerable<FileDto> values = nativeFileDataSet.Tables[0].AsEnumerable().Select(x => new FileDto
			{
				DocumentArtifactID = (int)x[_DOCUMENT_ARTIFACT_ID_COLUMN],
				Location = x[_LOCATION_COLUMN].ToString(),
				FileName = x[_FILE_NAME_COLUMN].ToString(),
				FileSize = (long)x[_FILE_SIZE_COLUMN]
			});
			return values.ToList();
		}
	}
}
