using System.Collections.Generic;
using kCura.IntegrationPoints.Domain.Models;
using kCura.Relativity.Client;

namespace kCura.IntegrationPoints.Core.Helpers
{
	public interface ITreeByParentIdCreator
	{
		JsTreeItemDTO Create(IList<Artifact> artifacts);
	}
}