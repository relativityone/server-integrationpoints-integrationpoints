using System.Collections.Generic;
using Relativity.Core.DTO;

namespace kCura.IntegrationPoint.Tests.Core.TestHelpers
{
	public interface IProductionImagesService
	{
		IList<File> GetProductionImagesFileInfo(int workspaceId, int documentArtifactId);
	}
}