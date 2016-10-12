using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Domain.Extensions;
using kCura.IntegrationPoints.Domain.Models;
using kCura.Relativity.Client;
using Relativity.API;
using Relativity.Services.Exceptions;

namespace kCura.IntegrationPoints.Core.Helpers.Implementations
{
	public class ArtifactTreeCreator : IArtifactTreeCreator
	{
		private readonly IAPILog _logger;

		public ArtifactTreeCreator(IHelper helper)
		{
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<IHelper>();
		}

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
				LogMissingRootError();
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

		#region Logging

		private void LogMissingRootError()
		{
			_logger.LogError("Root for tree not found.");
		}

		#endregion
	}
}