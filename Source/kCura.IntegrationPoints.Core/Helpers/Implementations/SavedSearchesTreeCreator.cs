using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Domain.Extensions;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.Core.Service;
using Relativity.Services.Search;

namespace kCura.IntegrationPoints.Core.Helpers.Implementations
{
    public class SavedSearchesTreeCreator : ISavedSearchesTreeCreator
    {
        private const string SanitizedSavedSearchDefaultName = "Sanitized Search Name";
        private readonly IHtmlSanitizerManager _htmlSanitizerManager;

        public SavedSearchesTreeCreator(IHtmlSanitizerManager htmlSanitizerManager)
        {
            _htmlSanitizerManager = htmlSanitizerManager;
        }

        public JsTreeItemDTO Create(IEnumerable<SearchContainerItem> nodes)
        {
            return CreateWithChildrenImpl(nodes, Enumerable.Empty<SavedSearchContainerItem>());
        }

        public JsTreeItemDTO Create(IEnumerable<SearchContainerItem> nodes, IEnumerable<SavedSearchContainerItem> children)
        {
            return CreateWithChildrenImpl(nodes, children);
        }

        private string GetSanitizedText(string text)
        {
            SanitizeResult sanitizedResult = _htmlSanitizerManager.Sanitize(text);
            return sanitizedResult.HasErrors || string.IsNullOrWhiteSpace(sanitizedResult.CleanHTML)
                ? SanitizedSavedSearchDefaultName
                : sanitizedResult.CleanHTML;
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
                    Icon = JsTreeItemIconEnum.SavedSearchFolder.GetDescription()
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
                            sort = x.Text.CompareTo(y.Text);
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