using System.Collections.Generic;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.Services.Search;

namespace kCura.IntegrationPoints.Core.Helpers
{
    public interface ISavedSearchesTreeCreator : ITreeByParentIdCreator<SearchContainerItem>
    {
        JsTreeItemDTO Create(IEnumerable<SearchContainerItem> nodes, IEnumerable<SavedSearchContainerItem> children);

        JsTreeItemDTO CreateTreeForNodeAndDirectChildren(SearchContainer searchContainer, IEnumerable<SearchContainerItem> directories,
            IEnumerable<SavedSearchContainerItem> items);
    }
}