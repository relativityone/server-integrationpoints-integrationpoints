using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Domain.Extensions;
using kCura.IntegrationPoints.Domain.Models;
using kCura.Relativity.Client;
using Relativity.Services.Exceptions;

namespace kCura.IntegrationPoints.Core.Helpers.Implementations
{
    public class ArtifactTreeCreator : IArtifactTreeCreator
    {
        public JsTreeItemDTO Create(IEnumerable<Artifact> nodes)
        {
            var treeItemsFlat = ConvertToTreeItems(nodes);
            var root = FindRoot(treeItemsFlat);
            root.Icon = JsTreeItemIconEnum.Root.GetDescription();

            BuildTree(treeItemsFlat);

            return root;
        }

        private IList<JsTreeItemWithParentIdDTO> ConvertToTreeItems(IEnumerable<Artifact> nodes)
        {
            return nodes.Select(x => x.ToTreeItemWithParentIdDTO()).ToList();
        }

        private JsTreeItemDTO FindRoot(IList<JsTreeItemWithParentIdDTO> treeItemsFlat)
        {
            var ids = treeItemsFlat.Select(x => x.Id).ToList();
            var root = treeItemsFlat.Where(x => string.IsNullOrEmpty(x.ParentId) || !ids.Contains(x.ParentId)).ToList();

            if (root.Count != 1)
            {
                throw new NotFoundException("Root not found");
            }

            return root[0];
        }

        private void BuildTree(IList<JsTreeItemWithParentIdDTO> treeItemsFlat)
        {
            foreach (var treeItem in treeItemsFlat)
            {
                var children = treeItemsFlat.Where(ch => ch.ParentId == treeItem.Id);
                treeItem.Children.AddRange(children);
            }
        }
    }
}