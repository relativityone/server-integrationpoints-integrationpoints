using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using kCura.Config;
using kCura.IntegrationPoints.Common.Constants;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.WinEDDS.Service.Export;
using Relativity.Services.Interfaces.File;
using Relativity.Services.Interfaces.File.Models;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class FileRepository : IFileRepository
	{
		private readonly Func<ISearchManager> _searchManagerFactory;
		private readonly IExternalServiceInstrumentationProvider _instrumentationProvider;
		private const string ImageLocationColumn = "Location";

		public FileRepository(
			Func<ISearchManager> searchManagerFactory, 
			IExternalServiceInstrumentationProvider instrumentationProvider)
		{
			_searchManagerFactory = searchManagerFactory;
			_instrumentationProvider = instrumentationProvider;
		}

		public List<string> GetImagesForProductionDocuments(
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
				return ToLocationList(instrumentation.Execute<DataSet>(
					() => searchManager.RetrieveImagesForProductionDocuments(workspaceID, documentIDs,
						productionID)));
			}
		}

		public List<string> GetImagesForDocuments(int workspaceID, int[] documentIDs)
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
				return ToLocationList(instrumentation.Execute<DataSet>(
					() => searchManager.RetrieveImagesForDocuments(workspaceID, documentIDs)));
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
			return values.Select(x => (x[ImageLocationColumn] ?? string.Empty).ToString()).ToList();
		}
	}
}
