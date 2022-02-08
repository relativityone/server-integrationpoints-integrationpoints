using System.Collections.Generic;
using System.Linq;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Services.Objects.Exceptions;

namespace Relativity.IntegrationPoints.Tests.Integration.Helpers.WorkspaceHelpers
{
    public class DocumentHelper : WorkspaceHelperBase
    {
        private readonly SavedSearchHelper _savedSearchHelper;
        private readonly ProductionHelper _productionHelper;

        public DocumentHelper(WorkspaceTest workspace) : base(workspace)
        {
            _savedSearchHelper = Workspace.Helpers.SavedSearchHelper;
            _productionHelper = Workspace.Helpers.ProductionHelper;
        }

        public IList<DocumentTest> GetAllDocuments()
        {
            return Workspace.Documents;
        }

        public IList<DocumentTest> GetDocumentsWithoutImagesNativesAndFields()
        {
            return Workspace.Documents.Where(x => !x.HasNatives && !x.HasImages && !x.HasFields).ToList();
        }

        public IList<DocumentTest> GetDocumentsForSavedSearchId(int savedSearchId)
        {
            SavedSearchTest savedSearch = _savedSearchHelper.GetSavedSearch(savedSearchId);
            IList<DocumentTest> documents = GetDocumentsBySearchCriteria(savedSearch.SearchCriteria);

            return documents;
        }

        public int GetImagesSizeForSavedSearch(int savedSearchId)
        {
            SavedSearchTest savedSearch = _savedSearchHelper.GetSavedSearch(savedSearchId);
            int imagesSize = GetImagesSizeBySearchCriteria(savedSearch.SearchCriteria);

            return imagesSize;
        }

        public int GetImagesSizeForProduction(int productionId)
        {
            ProductionTest production = _productionHelper.GetProduction(productionId);
            SavedSearchTest savedSearch = _savedSearchHelper.GetSavedSearch(production.SavedSearchId);
            int imagesSize = GetImagesSizeBySearchCriteria(savedSearch.SearchCriteria);

            return imagesSize;
        }

        public int GetImagesSizeForFolderBySearchCriteria(FolderTest folder, SearchCriteria searchCriteria)
        {
            IList<DocumentTest> documents = Workspace.Documents.Where(x =>
                x.HasImages == searchCriteria.HasImages &&
                x.HasNatives == searchCriteria.HasNatives &&
                x.HasFields == searchCriteria.HasFields &&
                x.FolderName == folder.Name).ToList();

            if (!documents.Any())
            {
                throw new ArtifactNotFoundException($"Documents for folder {folder.Name} not found");
            }

            int imagesSize = CalculateDocumentsImagesSize(documents);

            return imagesSize;
        }

        private int GetImagesSizeBySearchCriteria(SearchCriteria searchCriteria)
        {
            IEnumerable<DocumentTest> documents = GetDocumentsBySearchCriteria(searchCriteria);

            int imagesSize = CalculateDocumentsImagesSize(documents);

            return imagesSize;
        }

        private IList<DocumentTest> GetDocumentsBySearchCriteria(SearchCriteria searchCriteria)
        {
            IList<DocumentTest> documents = Workspace.Documents.Where(x =>
                x.HasImages == searchCriteria.HasImages &&
                x.HasNatives == searchCriteria.HasNatives &&
                x.HasFields == searchCriteria.HasFields).ToList();

            if (!documents.Any())
            {
                throw new ArtifactNotFoundException("Documents for folder searchCriteria not found");
            }

            return documents;
        }

        private int CalculateDocumentsImagesSize(IEnumerable<DocumentTest> documents)
        {
            return documents.Select(x => x.ImageCount).Count();
        }

    }
}
