using System;
using System.Data;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;
using kCura.WinEDDS.Service.Export;
using Relativity.API;
using Relativity.Services.Interfaces.File;
using Relativity.Services.Interfaces.File.Models;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	/// <summary>
	/// Implementation above the original FileRepository for creating and disposing IsearchManager kepler proxy.
	/// It will be no longer required when we move all dependencies to the IoC container.
	/// </summary>
	public class DisposableFileRepository : IFileRepository
	{
		private readonly IServiceFactory _serviceFactory;
		private readonly IExternalServiceInstrumentationProvider _instrumentationProvider;
		private readonly CreateFileRepositoryDelegate _createFileRepositoryDelegate;

		public delegate IFileRepository CreateFileRepositoryDelegate(
			ISearchManager searchManager,
			IExternalServiceInstrumentationProvider instrumentationProvider
		);

		public DisposableFileRepository(
			IServiceFactory serviceFactory,
			IExternalServiceInstrumentationProvider instrumentationProvider,
			CreateFileRepositoryDelegate createFileRepositoryDelegate)
		{
			_serviceFactory = serviceFactory;
			_instrumentationProvider = instrumentationProvider;
			_createFileRepositoryDelegate = createFileRepositoryDelegate;
		}

		public DataSet GetNativesForSearch(int workspaceID, int[] documentIDs)
		{
			using (ISearchManager searchManager = CreateSearchManager())
			{
				return _createFileRepositoryDelegate(searchManager, _instrumentationProvider)
					.GetNativesForSearch(workspaceID, documentIDs);
			}
		}

		public DataSet GetNativesForProduction(int workspaceID, int productionID, int[] documentIDs)
		{
			using (ISearchManager searchManager = CreateSearchManager())
			{
				return _createFileRepositoryDelegate(searchManager, _instrumentationProvider)
					.GetNativesForProduction(workspaceID, productionID, documentIDs);
			}
		}

		public DataSet GetImagesForProductionDocuments(int workspaceID, int productionID, int[] documentIDs)
		{
			using (ISearchManager searchManager = CreateSearchManager())
			{
				return _createFileRepositoryDelegate(searchManager, _instrumentationProvider)
					.GetImagesForProductionDocuments(workspaceID, productionID, documentIDs);
			}
		}

		public DataSet GetImagesForDocuments(int workspaceID, int[] documentIDs)
		{
			using (ISearchManager searchManager = CreateSearchManager())
			{
				return _createFileRepositoryDelegate(searchManager, _instrumentationProvider)
					.GetImagesForDocuments(workspaceID, documentIDs);
			}
		}

		public DataSet GetProducedImagesForDocument(int workspaceID, int documentID)
		{
			using (ISearchManager searchManager = CreateSearchManager())
			{
				return _createFileRepositoryDelegate(searchManager, _instrumentationProvider)
					.GetProducedImagesForDocument(workspaceID, documentID);
			}
		}

		public DataSet GetImagesForExport(int workspaceID, int[] productionIDs, int[] documentIDs)
		{
			using (ISearchManager searchManager = CreateSearchManager())
			{
				return _createFileRepositoryDelegate(searchManager, _instrumentationProvider)
					.GetImagesForExport(workspaceID, productionIDs, documentIDs);
			}
		}

		private ISearchManager CreateSearchManager()
		{
			return _serviceFactory.CreateSearchManager();
		}
	}
}
