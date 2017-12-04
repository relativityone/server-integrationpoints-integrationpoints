using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.Data;
using Relativity.Core;

namespace kCura.IntegrationPoints.Data.Repositories
{
	public interface IFileRepository
	{
		DataView RetrieveAllImagesForDocuments(int documentArtifactId);
		DataView RetrieveImagesByProductionArtifactIDForProductionExportByDocumentSet(int productionArtifactID, int documentArtifactId);
	}
}
