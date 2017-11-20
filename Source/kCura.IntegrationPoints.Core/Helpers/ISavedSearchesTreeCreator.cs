using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.Core.Service;
using Relativity.Services.Search;

namespace kCura.IntegrationPoints.Core.Helpers
{
    public interface ISavedSearchesTreeCreator : ITreeByParentIdCreator<SearchContainerItem>
    {
        JsTreeItemDTO Create(IEnumerable<SearchContainerItem> nodes, IEnumerable<SavedSearchContainerItem> children);
    }
}