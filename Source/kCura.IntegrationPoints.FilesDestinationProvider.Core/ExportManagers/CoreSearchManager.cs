using System.Data;
using System.Linq;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Helpers;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Repositories;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Utilities;
using kCura.WinEDDS;
using kCura.WinEDDS.Service.Export;
using Relativity.Core;
using Relativity.Core.Api.Manager.UI;
using Relativity.Core.Service;
using Relativity.Services.Interfaces.ViewField.Models;
using RelativityViewFieldInfo = Relativity.ViewFieldInfo;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.ExportManagers
{
	public class CoreSearchManager : ISearchManager
	{
		private readonly BaseServiceContext _baseServiceContext;
		private readonly IFileRepository _fileRepository;
		private readonly IViewFieldRepository _viewFieldRepository;

		public CoreSearchManager( BaseServiceContext baseServiceContext, IFileRepository fileRepository, IViewFieldRepository viewFieldRepository)
		{
			_baseServiceContext = baseServiceContext;
			_fileRepository = fileRepository;
			_viewFieldRepository = viewFieldRepository;
		}

		public DataSet RetrieveNativesForSearch(int caseContextArtifactID, string documentArtifactIDs)
		{
			var a = new DataTable();
			int[] documentIDs =
				CommaSeparatedNumbersToArrayConverter.ConvertCommaSeparatedStringToArrayOfInts(documentArtifactIDs);
			return _fileRepository.GetNativesForSearchAsync(caseContextArtifactID, documentIDs).ToDataSet("DataTable");
		}

		public DataSet RetrieveNativesForProduction(int caseContextArtifactID, int productionArtifactID,
			string documentArtifactIDs)
		{
			int[] documentIDs =
				CommaSeparatedNumbersToArrayConverter.ConvertCommaSeparatedStringToArrayOfInts(documentArtifactIDs);
			return _fileRepository.GetNativesForProductionAsync(caseContextArtifactID, productionArtifactID, documentIDs).ToDataSet("DataTable");

		}

		public DataSet RetrieveFilesForDynamicObjects(int caseContextArtifactID, int fileFieldArtifactID,
			int[] objectIds)
		{
			Init(caseContextArtifactID);
			return FileQuery.RetrieveFilesForDynamicObjects(_baseServiceContext, fileFieldArtifactID, objectIds)
				?.ToDataSet();
		}

		public DataSet RetrieveImagesForProductionDocuments(int caseContextArtifactID, int[] documentArtifactIDs,
			int productionArtifactID)
		{
			return _fileRepository
				.GetImagesForProductionDocumentsAsync(caseContextArtifactID, productionArtifactID, documentArtifactIDs)
				.ToDataSet("DataTable");

		}

		public DataSet RetrieveImagesForDocuments(int caseContextArtifactID, int[] documentArtifactIDs)
		{
			return _fileRepository.GetImagesForDocumentsAsync(caseContextArtifactID, documentArtifactIDs)
				.ToDataSet("DataTable");
		}

		public DataSet RetrieveProducedImagesForDocument(int caseContextArtifactID, int documentArtifactID)
		{
			return _fileRepository.GetProducedImagesForDocumentAsync(caseContextArtifactID, documentArtifactID)
				.ToDataSet("TableName");
		}

		public DataSet RetrieveImagesByProductionIDsAndDocumentIDsForExport(int caseContextArtifactID,
			int[] productionArtifactIDs, int[] documentArtifactIDs)
		{
			return _fileRepository
				.GetImagesForExportAsync(caseContextArtifactID, productionArtifactIDs, documentArtifactIDs)
				.ToDataSet("DataTable");
		}

		public ViewFieldInfo[] RetrieveAllExportableViewFields(int caseContextArtifactID, int artifactTypeID)
		{
			ViewFieldResponse[] viewFieldResponseArray =
				_viewFieldRepository.ReadExportableViewFields(caseContextArtifactID, artifactTypeID);
			ViewFieldInfo[] viewFieldInfoArray = ToViewFieldInfoArray(viewFieldResponseArray);
			return viewFieldInfoArray;
		}

		private static ViewFieldInfo[] ToViewFieldInfoArray(ViewFieldResponse[] viewFieldResponseArray)
		{
			return viewFieldResponseArray
				.Select(ToViewFieldInfo)
				.ToArray();
		}

		private static ViewFieldInfo ToViewFieldInfo(ViewFieldResponse viewFieldResponse)
		{
			RelativityViewFieldInfo coreViewFieldInfo = new CoreViewFieldInfo(viewFieldResponse);
			ViewFieldInfo viewFieldInfo = new ViewFieldInfo(coreViewFieldInfo);
			return viewFieldInfo;
		}

		public int[] RetrieveDefaultViewFieldIds(int caseContextArtifactID, int viewArtifactID, int artifactTypeID,
			bool isProduction)
		{
			ViewFieldIDResponse[] viewFieldIDResponseArray = isProduction
				? _viewFieldRepository.ReadViewFieldIDsFromProduction(caseContextArtifactID, artifactTypeID,
					viewArtifactID)
				: _viewFieldRepository.ReadViewFieldIDsFromSearch(caseContextArtifactID, artifactTypeID,
					viewArtifactID);

			return viewFieldIDResponseArray
				.Where(x => x.ArtifactID.Equals(viewArtifactID))
				.Select(x => x.ArtifactViewFieldID)
				.ToArray();
		}

		public DataSet RetrieveViewsByContextArtifactID(int caseContextArtifactID, int artifactTypeID, bool isSearch)
		{
			return InitViewManager(caseContextArtifactID)
				.ExternalRetrieveViews(_baseServiceContext, artifactTypeID, isSearch);
		}

		public void Dispose()
		{
		}

		private ViewManager InitViewManager(int appArtifactId)
		{
			Init(appArtifactId);
			return new ViewManager();
		}

		private void Init(int appArtifactId)
		{
			_baseServiceContext.AppArtifactID = appArtifactId;
		}
	}
}