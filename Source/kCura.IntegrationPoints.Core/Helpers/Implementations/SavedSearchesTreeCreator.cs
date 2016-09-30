using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.Services.Search;

namespace kCura.IntegrationPoints.Core.Helpers.Implementations
{
    public class SavedSearchesTreeCreator : ISavedSearchesTreeCreator
    {
        public JsTreeItemDTO Create(IEnumerable<SearchContainerItem> nodes)
        {
            return CreateWithChildrenImpl(nodes, Enumerable.Empty<SavedSearchContainerItem>());
        }

        public JsTreeItemDTO Create(IEnumerable<SearchContainerItem> nodes, IEnumerable<SavedSearchContainerItem> children)
        {
            return CreateWithChildrenImpl(nodes, children);
        }

        private JsTreeItemDTO CreateWithChildrenImpl(IEnumerable<SearchContainerItem> nodes, IEnumerable<SavedSearchContainerItem> children)
        {
            // map folders to dictonary
            Dictionary<string, JsTreeItemWithParentIdDTO> folderLookup = nodes.Select(x =>
            {
                return new JsTreeItemWithParentIdDTO
                {
                    Id = x.SearchContainer.ArtifactID.ToString(),
                    ParentId = x.ParentContainer.ArtifactID.ToString(),
                    Text = x.SearchContainer.Name,
                    Icon = JsTreeItemIconType.SavedSearchFolder
                };
            }).ToDictionary(x => x.Id);

            // map searches to dictonary
            Dictionary<string, JsTreeItemWithParentIdDTO[]> childrenLookup = children.Select(x =>
            {
                return new JsTreeItemWithParentIdDTO
                {
                    Id = x.SavedSearch.ArtifactID.ToString(),
                    ParentId = x.ParentContainer.ArtifactID.ToString(),
                    Text = x.SavedSearch.Name,
                    Icon = x.Personal ? JsTreeItemIconType.SavedSearchPersonal : JsTreeItemIconType.SavedSearch
                };
            }).GroupBy(x => x.ParentId).ToDictionary(x => x.Key, x => x.ToArray());

            // hook up children with parents
            JsTreeItemWithParentIdDTO[] child;
            JsTreeItemWithParentIdDTO parent;
            foreach (JsTreeItemWithParentIdDTO item in folderLookup.Values)
            {
                if (childrenLookup.TryGetValue(item.Id, out child))
                {
                    item.Children.AddRange(child);
                }

                if (folderLookup.TryGetValue(item.ParentId, out parent))
                {
                    parent.Children.Add(item);
                }
            }

            // root node has a parent but one not referenced by other containers
            int rootParentId = nodes.Select(x => x.ParentContainer.ArtifactID)
                .Except(nodes.Select(x => x.SearchContainer.ArtifactID))
                .Single();

            // the one and only
            JsTreeItemWithParentIdDTO root = folderLookup.Values.Where(x => x.ParentId == rootParentId.ToString()).Single();
            root.Icon = JsTreeItemIconType.Folder;

            return root;
        }
    }
}