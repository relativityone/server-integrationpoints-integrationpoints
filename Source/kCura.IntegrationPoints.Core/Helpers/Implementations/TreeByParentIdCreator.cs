using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Domain.Extensions;
using kCura.IntegrationPoints.Domain.Models;
using kCura.Relativity.Client;
using Relativity.Services.Exceptions;

namespace kCura.IntegrationPoints.Core.Helpers.Implementations
{
    public class TreeByParentIdCreator : ITreeByParentIdCreator
    {
        public JsTreeItemDTO Create(IList<Artifact> artifacts)
        {
            var treeItemsFlat = ConvertToTreeItems(artifacts);
            var root = FindRoot(treeItemsFlat);
            BuildTree(treeItemsFlat);
            return root;
        }

        private IList<JsTreeItemWithParentIdDto> ConvertToTreeItems(IList<Artifact> artifacts)
        {
            return artifacts.Select(x => x.ToTreeItemWithParentIdDTO()).ToList();
        }

        private JsTreeItemDTO FindRoot(IList<JsTreeItemWithParentIdDto> treeItemsFlat)
        {
            var ids = treeItemsFlat.Select(x => x.Id).ToList();
            var root = treeItemsFlat.Where(x => string.IsNullOrEmpty(x.ParentId) || !ids.Contains(x.ParentId)).ToList();

            if (root.Count != 1)
            {
                throw new NotFoundException("Root not found");
            }
            return root[0];
        }

        private void BuildTree(IList<JsTreeItemWithParentIdDto> treeItemsFlat)
        {
            foreach (var treeItem in treeItemsFlat)
            {
                var children = treeItemsFlat.Where(ch => ch.ParentId == treeItem.Id);
                treeItem.Children.AddRange(children);
            }
        }
    }
}