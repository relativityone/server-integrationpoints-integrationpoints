using System.Collections.Generic;
using Relativity.Core.DTO;

namespace kCura.IntegrationPoint.Tests.Core.TestHelpers
{
	public interface IImagesService
	{
		IList<File> GetImagesFileInfo(int workspaceId, int documentArtifactId);
	}
}