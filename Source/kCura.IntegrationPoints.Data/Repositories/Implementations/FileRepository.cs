using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.Data;
using Relativity.Core;
using Relativity.Core.Service;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class FileRepository : IFileRepository
	{
		private readonly ICoreContext _coreContext;

		public FileRepository(ICoreContext coreContext)
		{
			_coreContext = coreContext;
		}

		public DataView RetrieveAllImagesForDocuments(int documentArtifactId)
		{
			return FileQuery.RetrieveAllImagesForDocuments(_coreContext, new[] { documentArtifactId });
		}

		public DataView RetrieveImagesByProductionArtifactIDForProductionExportByDocumentSet(int productionArtifactId, int documentArtifactId)
		{
			return FileQuery.RetrieveImagesByProductionArtifactIDForProductionExportByDocumentSet(_coreContext,
				productionArtifactId, new[] {documentArtifactId});
		}
	}
}
