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

        public DocumentHelper(WorkspaceFake workspace) : base(workspace)
        {
            _savedSearchHelper = Workspace.Helpers.SavedSearchHelper;
            _productionHelper = Workspace.Helpers.ProductionHelper;
        }

        public IList<DocumentFake> GetAllDocuments()
        {
            return Workspace.Documents;
        }

        public IList<DocumentFake> GetDocumentsWithoutImagesNativesAndFields()
        {
            return Workspace.Documents.Where(x => !x.HasNatives && !x.HasImages && !x.HasFields).ToList();
        }

        public IList<DocumentFake> GetDocumentsForSavedSearchId(int savedSearchId)
        {
            SavedSearchFake savedSearch = _savedSearchHelper.GetSavedSearch(savedSearchId);
            IList<DocumentFake> documents = GetDocumentsBySearchCriteria(savedSearch.SearchCriteria);

            return documents;
        }

        public int GetImagesSizeForSavedSearch(int savedSearchId)
        {
            SavedSearchFake savedSearch = _savedSearchHelper.GetSavedSearch(savedSearchId);
            int imagesSize = GetImagesSizeBySearchCriteria(savedSearch.SearchCriteria);

            return imagesSize;
        }

        public int GetImagesSizeForProduction(int productionId)
        {
            ProductionFake production = _productionHelper.GetProduction(productionId);
            SavedSearchFake savedSearch = _savedSearchHelper.GetSavedSearch(production.SavedSearchId);
            int imagesSize = GetImagesSizeBySearchCriteria(savedSearch.SearchCriteria);

            return imagesSize;
        }

        public int GetImagesSizeForFolderBySearchCriteria(FolderFake folder, SearchCriteria searchCriteria)
        {
            IList<DocumentFake> documents = Workspace.Documents.Where(x =>
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
            IEnumerable<DocumentFake> documents = GetDocumentsBySearchCriteria(searchCriteria);

            int imagesSize = CalculateDocumentsImagesSize(documents);

            return imagesSize;
        }

        private IList<DocumentFake> GetDocumentsBySearchCriteria(SearchCriteria searchCriteria)
        {
            IList<DocumentFake> documents = Workspace.Documents.Where(x =>
                x.HasImages == searchCriteria.HasImages &&
                x.HasNatives == searchCriteria.HasNatives &&
                x.HasFields == searchCriteria.HasFields).ToList();

            if (!documents.Any())
            {
                throw new ArtifactNotFoundException("Documents for folder searchCriteria not found");
            }

            return documents;
        }

        private int CalculateDocumentsImagesSize(IEnumerable<DocumentFake> documents)
        {
            return documents.Select(x => x.ImageCount).Count();
        }

    }
}
