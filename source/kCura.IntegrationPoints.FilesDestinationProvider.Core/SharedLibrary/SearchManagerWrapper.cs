using System.Data;
using kCura.WinEDDS;
using kCura.WinEDDS.Service;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary
{
    internal class SearchManagerWrapper : ISearchManager
    {
        private readonly SearchManager _searchManager;

        public SearchManagerWrapper(SearchManager searchManager)
        {
            _searchManager = searchManager;
        }

        public DataSet RetrieveNativesForSearch(int caseContextArtifactID, string documentArtifactIDs)
        {
            return _searchManager.RetrieveNativesForSearch(caseContextArtifactID, documentArtifactIDs);
        }

        public DataSet RetrieveNativesForProduction(int caseContextArtifactID, int productionArtifactID,
            string documentArtifactIDs)
        {
            return _searchManager.RetrieveNativesForProduction(caseContextArtifactID, productionArtifactID,
                documentArtifactIDs);
        }

        public DataSet RetrieveFilesForDynamicObjects(int caseContextArtifactID, int fileFieldArtifactID,
            int[] objectIds)
        {
            return _searchManager.RetrieveFilesForDynamicObjects(caseContextArtifactID, fileFieldArtifactID, objectIds);
        }

        public DataSet RetrieveImagesForProductionDocuments(int caseContextArtifactID, int[] documentArtifactIDs,
            int productionArtifactID)
        {
            return _searchManager.RetrieveImagesForProductionDocuments(caseContextArtifactID, documentArtifactIDs,
                productionArtifactID);
        }

        public DataSet RetrieveImagesForDocuments(int caseContextArtifactID, int[] documentArtifactIDs)
        {
            return _searchManager.RetrieveImagesForDocuments(caseContextArtifactID, documentArtifactIDs);
        }

        public DataSet RetrieveImagesByProductionIDsAndDocumentIDsForExport(int caseContextArtifactID,
            int[] productionArtifactIDs, int[] documentArtifactIDs)
        {
            return _searchManager.RetrieveImagesByProductionIDsAndDocumentIDsForExport(caseContextArtifactID,
                productionArtifactIDs, documentArtifactIDs);
        }

        public ViewFieldInfo[] RetrieveAllExportableViewFields(int caseContextArtifactID, int artifactTypeID)
        {
            return _searchManager.RetrieveAllExportableViewFields(caseContextArtifactID, artifactTypeID);
        }

        public void Dispose()
        {
            _searchManager.Dispose();
        }
    }
}