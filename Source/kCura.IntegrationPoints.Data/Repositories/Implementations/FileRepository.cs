using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Common.Constants;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;
using kCura.WinEDDS.Service.Export;
using Relativity.Services.Interfaces.File;
using Relativity.Services.Interfaces.File.Models;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class FileRepository : IFileRepository
	{
		private readonly ISearchManager _searchManager;
		private readonly IExternalServiceInstrumentationProvider _instrumentationProvider;

		public FileRepository(
			ISearchManager searchManager, 
			IExternalServiceInstrumentationProvider instrumentationProvider)
		{
			_searchManager = searchManager;
			_instrumentationProvider = instrumentationProvider;
		}

		public DataSet GetNativesForSearch(int workspaceID, int[] documentIDs)
		{
			ThrowWhenNullArgument(documentIDs, nameof(documentIDs));

			if (!documentIDs.Any())
			{
				return new DataSet("empty");
			}

			IExternalServiceSimpleInstrumentation instrumentation = CreateInstrumentation(
				operationName: nameof(ISearchManager.RetrieveNativesForSearch)
			);
			string docIDs = string.Join(",", documentIDs);
			return instrumentation.Execute<DataSet>(
				() => _searchManager.RetrieveNativesForSearch(workspaceID, docIDs));
		}

		public DataSet GetNativesForProduction(
			int workspaceID, 
			int productionID, 
			int[] documentIDs)
		{
			ThrowWhenNullArgument(documentIDs, nameof(documentIDs));

			if (!documentIDs.Any())
			{
				return new DataSet("empty");
			}


			IExternalServiceSimpleInstrumentation instrumentation = CreateInstrumentation(
				operationName: nameof(ISearchManager.RetrieveNativesForProduction)
			);

			string docIDs = string.Join(",", documentIDs);
			return instrumentation.Execute<DataSet>(
				() => _searchManager.RetrieveNativesForProduction(workspaceID, 
					productionID,
					docIDs));
		}

		public DataSet GetImagesForProductionDocuments(
			int workspaceID, 
			int productionID,
			int[] documentIDs)
		{
			ThrowWhenNullArgument(documentIDs, nameof(documentIDs));

			if (!documentIDs.Any())
			{
				return new DataSet("empty");
			}

			IExternalServiceSimpleInstrumentation instrumentation = CreateInstrumentation(
				operationName: nameof(ISearchManager.RetrieveImagesForProductionDocuments)
			);

			return instrumentation.Execute<DataSet>(
				() => _searchManager.RetrieveImagesForProductionDocuments(workspaceID, documentIDs, productionID));
		}

		public DataSet GetImagesForDocuments(int workspaceID, int[] documentIDs)
		{
			ThrowWhenNullArgument(documentIDs, nameof(documentIDs));

			if (!documentIDs.Any())
			{
				return new DataSet("empty");
			}

			IExternalServiceSimpleInstrumentation instrumentation = CreateInstrumentation(
				operationName: nameof(ISearchManager.RetrieveImagesForDocuments)
			);

			return instrumentation.Execute<DataSet>(
				() => _searchManager.RetrieveImagesForDocuments(workspaceID, documentIDs));
		}

		public DataSet GetProducedImagesForDocument(int workspaceID, int documentID)
		{
			IExternalServiceSimpleInstrumentation instrumentation = CreateInstrumentation(
				operationName: nameof(ISearchManager.RetrieveProducedImagesForDocument)
			);

			return instrumentation.Execute<DataSet>(
				() => _searchManager.RetrieveProducedImagesForDocument(workspaceID, documentID));
		}

		public DataSet GetImagesForExport(int workspaceID, int[] productionIDs, int[] documentIDs)
		{
			ThrowWhenNullArgument(productionIDs, nameof(productionIDs));
			ThrowWhenNullArgument(documentIDs, nameof(documentIDs));

			if (!productionIDs.Any() || !documentIDs.Any())
			{
				return new DataSet("empty");
			}

			IExternalServiceSimpleInstrumentation instrumentation = CreateInstrumentation(
				operationName: nameof(ISearchManager.RetrieveImagesByProductionIDsAndDocumentIDsForExport)
			);

			return instrumentation.Execute<DataSet>(
				() => _searchManager.RetrieveImagesByProductionIDsAndDocumentIDsForExport(workspaceID, productionIDs, documentIDs));
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
	}
}
