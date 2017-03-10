using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.ImportProvider.Parser.Interfaces
{
	public interface IImportFileLocationService
	{
		string ErrorFilePath(int integrationPointArtifactId);
		string LoadFileFullPath(int integrationPointArtifactId);
	}
}
