using System.Collections.Generic;
using kCura.IntegrationPoints.Domain.Models;
using kCura.Relativity.Client;

namespace kCura.IntegrationPoints.Core.Helpers
{
	public interface ITreeByParentIdCreator
	{
		TreeItemDTO Create(IList<Artifact> artifacts);
	}
}