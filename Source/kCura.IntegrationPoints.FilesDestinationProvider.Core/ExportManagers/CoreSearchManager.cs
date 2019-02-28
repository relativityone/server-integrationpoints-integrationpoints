using System.Collections.Generic;
using System.Data;
using kCura.WinEDDS;
using kCura.WinEDDS.Service.Export;
using Relativity.Core;
using Relativity.Core.Service;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.ExportManagers
{
	public class CoreSearchManager : ISearchManager
	{
		private readonly BaseServiceContext _baseServiceContext;

		public CoreSearchManager(BaseServiceContext baseServiceContext)
		{
			_baseServiceContext = baseServiceContext;
		}

		public DataSet RetrieveNativesForSearch(int caseContextArtifactID, string documentArtifactIDs)
		{
			Init(caseContextArtifactID);
			return FileQuery.RetrieveNativesForDocuments(_baseServiceContext, documentArtifactIDs).ToDataSet();
		}

		public DataSet RetrieveNativesForProduction(int caseContextArtifactID, int productionArtifactID, string documentArtifactIDs)
		{
			Init(caseContextArtifactID);
			return FileQuery.RetrieveNativesForProductionDocuments(_baseServiceContext, productionArtifactID, documentArtifactIDs).ToDataSet();
		}

		public DataSet RetrieveFilesForDynamicObjects(int caseContextArtifactID, int fileFieldArtifactID, int[] objectIds)
		{
			Init(caseContextArtifactID);
			return FileQuery.RetrieveFilesForDynamicObjects(_baseServiceContext, fileFieldArtifactID, objectIds)?.ToDataSet();
		}

		public DataSet RetrieveImagesForProductionDocuments(int caseContextArtifactID, int[] documentArtifactIDs, int productionArtifactID)
		{
			return FileQuery.RetrieveImagesByProductionArtifactIDForProductionExportByDocumentSet(_baseServiceContext, productionArtifactID, documentArtifactIDs).ToDataSet();
		}

		public DataSet RetrieveImagesForDocuments(int caseContextArtifactID, int[] documentArtifactIDs)
		{
			return FileQuery.RetrieveAllImagesForDocuments(_baseServiceContext, documentArtifactIDs).ToDataSet();
		}

		public DataSet RetrieveProducedImagesForDocument(int caseContextArtifactID, int documentArtifactID)
		{
			return FileQuery.RetrieveAllByDocumentArtifactIdAndType(_baseServiceContext, documentArtifactID, 3).ToDataSet(); // TODO 3 -> Type enum
		}

		public DataSet RetrieveImagesByProductionIDsAndDocumentIDsForExport(int caseContextArtifactID, int[] productionArtifactIDs, int[] documentArtifactIDs)
		{
			return FileQuery.RetrieveByProductionIDsAndDocumentIDsForExport(_baseServiceContext, productionArtifactIDs, documentArtifactIDs).ToDataSet();
		}

		public ViewFieldInfo[] RetrieveAllExportableViewFields(int caseContextArtifactID, int artifactTypeID)
		{
			var allExportableViewFieldsDataSet = InitSearchQuery(caseContextArtifactID).RetrieveAllExportableViewFields(_baseServiceContext, artifactTypeID).ToDataSet();
			var allExportableViewFieldsRows = allExportableViewFieldsDataSet.Tables[0].Rows;

			var result = new ViewFieldInfo[allExportableViewFieldsRows.Count];
			for (var i = 0; i < result.Length; i++)
			{
				result[i] = new ViewFieldInfo(allExportableViewFieldsRows[i]);
			}
			return result;
		}

		public int[] RetrieveDefaultViewFieldIds(int caseContextArtifactID, int viewArtifactID, int artifactTypeID, bool isProduction)
		{
			var avfLookupByArtifactIdDataSet =
				InitSearchQuery(caseContextArtifactID).RetrieveOrderedAvfLookupByArtifactIdList(_baseServiceContext, artifactTypeID, new[] {viewArtifactID}, isProduction)
					.ToDataSet();
			var avfLookupByArtifactIdRows = avfLookupByArtifactIdDataSet.Tables[0].Rows;

			var result = new List<int>();
			foreach (DataRow avfLookupByArtifactIdRow in avfLookupByArtifactIdRows)
			{
				if (viewArtifactID.Equals(avfLookupByArtifactIdRow["ArtifactID"]))
				{
					result.Add(int.Parse(avfLookupByArtifactIdRow["ArtifactViewFieldID"].ToString()));
				}
			}
			return result.ToArray();
		}

		public DataSet RetrieveViewsByContextArtifactID(int caseContextArtifactID, int artifactTypeID, bool isSearch)
		{
			return InitViewManager(caseContextArtifactID).ExternalRetrieveViews(_baseServiceContext, artifactTypeID, isSearch);
		}

		public void Dispose()
		{
		}

		private ISearchQuery InitSearchQuery(int appArtifactId)
		{
			Init(appArtifactId);
			return new SearchManager().Query;
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