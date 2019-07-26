﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using kCura.IntegrationPoints.Common;
using kCura.IntegrationPoints.Common.Constants;
using kCura.IntegrationPoints.Common.Handlers;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;
using kCura.IntegrationPoints.Data.Repositories.DTO;
using kCura.WinEDDS.Service.Export;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class FileRepository : IFileRepository
	{
		private const string _DOCUMENT_ARTIFACT_ID_COLUMN = "DocumentArtifactID";
		private const string _FILE_NAME_COLUMN = "Filename";
		private const string _LOCATION_COLUMN = "Location";
		private const string _FILE_SIZE_COLUMN = "Size";
		private const ushort _MAX_NUMBER_OF_RETRIES = 3;
		private const ushort _EXPONENTIAL_WAIT_TIME_BASE_IN_SEC = 3;

		private readonly Func<ISearchManager> _searchManagerFactory;
		private readonly IExternalServiceInstrumentationProvider _instrumentationProvider;
		private readonly IRetryHandler _retryHandler;

		public FileRepository(
			Func<ISearchManager> searchManagerFactory,
			IExternalServiceInstrumentationProvider instrumentationProvider,
			IRetryHandlerFactory retryHandlerFactory)
		{
			_retryHandler = retryHandlerFactory.Create(_MAX_NUMBER_OF_RETRIES, _EXPONENTIAL_WAIT_TIME_BASE_IN_SEC);
			_searchManagerFactory = searchManagerFactory;
			_instrumentationProvider = instrumentationProvider;
		}

		public List<string> GetImagesLocationForProductionDocuments(
			int workspaceID,
			int productionID,
			int[] documentIDs)
		{
			ThrowWhenNullArgument(documentIDs, nameof(documentIDs));

			if (!documentIDs.Any())
			{
				return new List<string>();
			}

			IExternalServiceSimpleInstrumentation instrumentation = CreateInstrumentation(
				operationName: nameof(ISearchManager.RetrieveImagesForProductionDocuments)
			);
			using (ISearchManager searchManager = _searchManagerFactory())
			{
				List<string> fileLocations = ToLocationList(instrumentation.Execute(
					() => _retryHandler.ExecuteWithRetries(
						() => searchManager.RetrieveImagesForProductionDocuments(workspaceID, documentIDs, productionID))
				));
				return fileLocations;
			}
		}

		public List<string> GetImagesLocationForDocuments(int workspaceID, int[] documentIDs)
		{
			ThrowWhenNullArgument(documentIDs, nameof(documentIDs));

			if (!documentIDs.Any())
			{
				return new List<string>();
			}

			IExternalServiceSimpleInstrumentation instrumentation = CreateInstrumentation(
				operationName: nameof(ISearchManager.RetrieveImagesForDocuments)
			);
			using (ISearchManager searchManager = _searchManagerFactory())
			{
				List<string> fileLocations = ToLocationList(instrumentation.Execute(
					() => _retryHandler.ExecuteWithRetries(
						() => searchManager.RetrieveImagesForDocuments(workspaceID, documentIDs))
				));
				return fileLocations;
			}
		}

		public List<FileDto> GetNativesForDocuments(int workspaceID, int[] documentIDs)
		{
			ThrowWhenNullArgument(documentIDs, nameof(documentIDs));

			if (!documentIDs.Any())
			{
				return new List<FileDto>();
			}

			string documentIDsString = string.Join(",", documentIDs.Select(x => x.ToString()));

			IExternalServiceSimpleInstrumentation instrumentation = CreateInstrumentation(
				operationName: nameof(ISearchManager.RetrieveNativesForSearch)
			);
			using (ISearchManager searchManager = _searchManagerFactory())
			{
				List<FileDto> files = ToFileDtoList(instrumentation.Execute(
					() => _retryHandler.ExecuteWithRetries(
						() => searchManager.RetrieveNativesForSearch(workspaceID, documentIDsString))
				));
				return files;
			}
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

		private static List<string> ToLocationList(DataSet fileLocationDataSet)
		{
			IEnumerable<DataRow> values = fileLocationDataSet.Tables[0].AsEnumerable();
			return values.Select(x => x[_LOCATION_COLUMN].ToString()).ToList();
		}

		private static List<FileDto> ToFileDtoList(DataSet nativeFileDataSet)
		{
			IEnumerable<FileDto> values = nativeFileDataSet.Tables[0].AsEnumerable().Select(x => new FileDto
			{
				DocumentArtifactID = (int) x[_DOCUMENT_ARTIFACT_ID_COLUMN],
				Location = x[_LOCATION_COLUMN].ToString(),
				FileName = x[_FILE_NAME_COLUMN].ToString(),
				FileSize = (long) x[_FILE_SIZE_COLUMN]
			});
			return values.ToList();
		}
	}
}
