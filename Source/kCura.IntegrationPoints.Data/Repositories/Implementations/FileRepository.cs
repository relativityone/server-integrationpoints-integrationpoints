using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Common.Constants;
using kCura.IntegrationPoints.Common.Handlers;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;
using kCura.WinEDDS.Service.Export;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class FileRepository : IFileRepository
	{
		private readonly Func<ISearchManager> _searchManagerFactory;
		private readonly IExternalServiceInstrumentationProvider _instrumentationProvider;
		private readonly IRetryHandler _retryHandler;

		private const string ImageLocationColumn = "Location";

		public FileRepository(
			Func<ISearchManager> searchManagerFactory,
			IExternalServiceInstrumentationProvider instrumentationProvider,
			IRetryHandler retryHandler)
		{
			_retryHandler = retryHandler;
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
				List<string> fileLocations = ToLocationList(instrumentation.Execute<DataSet>(
					() => _retryHandler.ExecuteWithRetries<DataSet>(
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
				List<string> fileLocations = ToLocationList(instrumentation.Execute<DataSet>(
					() => _retryHandler.ExecuteWithRetries<DataSet>(
						() => searchManager.RetrieveImagesForDocuments(workspaceID, documentIDs))
					));
				return fileLocations;
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

		private List<string> ToLocationList(DataSet fileLocationDataSet)
		{
			var values = fileLocationDataSet.Tables[0].AsEnumerable();
			return values.Select(x => (x[ImageLocationColumn]).ToString()).ToList();
		}
	}
}
