using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core.Interfaces.TextSanitizer;
using kCura.IntegrationPoints.Domain.Extensions;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.Services.Search;

namespace kCura.IntegrationPoints.Core.Helpers.Implementations
{
    public class SavedSearchesTreeCreator : ISavedSearchesTreeCreator
    {
        private const string SanitizedSavedSearchDefaultName = "Sanitized Search Name";
        private readonly ITextSanitizer _textSanitizer;

        public SavedSearchesTreeCreator(ITextSanitizer textSanitizer)
        {
            _textSanitizer = textSanitizer;
        }

        public JsTreeItemDTO Create(IEnumerable<SearchContainerItem> nodes)
        {
            return CreateWithChildrenImpl(nodes, Enumerable.Empty<SavedSearchContainerItem>());
        }

        public JsTreeItemDTO Create(IEnumerable<SearchContainerItem> nodes, IEnumerable<SavedSearchContainerItem> children)
        {
            return CreateWithChildrenImpl(nodes, children);
        }

        public JsTreeItemDTO CreateTreeForNodeAndDirectChildren(SearchContainer searchContainer, IEnumerable<SearchContainerItem> directories, IEnumerable<SavedSearchContainerItem> items)
        {
            var output = new JsTreeItemDTO
            {
                Id = searchContainer.ArtifactID.ToString(),
                Text = GetSanitizedText(searchContainer.Name),
                Icon = JsTreeItemIconEnum.SavedSearchFolder.GetDescription(),
                IsDirectory = true,
                Children = new List<JsTreeItemDTO>()
            };

            foreach (SearchContainerItem directory in directories)
            {
                var searchFolderDto = new JsTreeItemDTO
                {
                    Id = directory.SearchContainer.ArtifactID.ToString(),
                    Text = GetSanitizedText(directory.SearchContainer.Name),
                    Icon = JsTreeItemIconEnum.SavedSearchFolder.GetDescription(),
                    IsDirectory = true
                };
                output.Children.Add(searchFolderDto);
            }

            foreach (SavedSearchContainerItem savedSearchContainerItem in items)
            {
                var savedSearchDto = new JsTreeItemDTO
                {
                    Id = savedSearchContainerItem.SavedSearch.ArtifactID.ToString(),
                    Text = GetSanitizedText(savedSearchContainerItem.SavedSearch.Name),
                    IsDirectory = false,
                    Icon = (savedSearchContainerItem.Personal ? JsTreeItemIconEnum.SavedSearchPersonal : JsTreeItemIconEnum.SavedSearch).GetDescription()
                };
                output.Children.Add(savedSearchDto);
            }

            return output;
        }

        private string GetSanitizedText(string text)
        {
            SanitizationResult sanitizedResult = _textSanitizer.Sanitize(text);
            return sanitizedResult.HasErrors || string.IsNullOrWhiteSpace(sanitizedResult.SanitizedText)
                ? SanitizedSavedSearchDefaultName
                : sanitizedResult.SanitizedText;
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
                    Text = GetSanitizedText(x.SearchContainer.Name),
                    Icon = JsTreeItemIconEnum.SavedSearchFolder.GetDescription(),
                    IsDirectory = true
                };
            }).OrderBy(v => v.Text).ToDictionary(x => x.Id);

            // map searches to dictonary
            Dictionary<string, JsTreeItemWithParentIdDTO[]> childrenLookup = children.Select(x =>
            {
                return new JsTreeItemWithParentIdDTO
                {
                    Id = x.SavedSearch.ArtifactID.ToString(),
                    ParentId = x.ParentContainer.ArtifactID.ToString(),
                    Text = GetSanitizedText(x.SavedSearch.Name),
                    Icon = (x.Personal ? JsTreeItemIconEnum.SavedSearchPersonal : JsTreeItemIconEnum.SavedSearch).GetDescription()
                };
            }).GroupBy(x => x.ParentId).ToDictionary(x => x.Key, x => x.OrderBy(v => v.Text).ToArray());

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
                    parent.Children.Sort((x, y) =>
                    {
                        int sort = x.Icon.GetValue<JsTreeItemIconEnum>().CompareTo(y.Icon.GetValue<JsTreeItemIconEnum>());
                        if (sort == 0)
                        {
                            sort = String.Compare(x.Text, y.Text, StringComparison.Ordinal);
                        }

                        return sort;
                    });
                }
            }

            // root node has a parent but one not referenced by other containers
            int rootParentId = nodes.Select(x => x.ParentContainer.ArtifactID)
                .Except(nodes.Select(x => x.SearchContainer.ArtifactID))
                .Single();

            // the one and only
            JsTreeItemWithParentIdDTO root = folderLookup.Values.Where(x => x.ParentId == rootParentId.ToString()).Single();
            root.Icon = JsTreeItemIconEnum.Root.GetDescription();

            return root;
        }
    }
}