using kCura.EDDS.WebAPI.FieldManagerBase;
using kCura.WinEDDS.Service.Export;
using Relativity.Core;
using Relativity.Core.Service;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary
{
	public class CoreFieldManager : IFieldManager
	{
		private readonly BaseServiceContext _baseServiceContext;

		public CoreFieldManager(BaseServiceContext baseServiceContext)
		{
			_baseServiceContext = baseServiceContext;
		}

		public Field Read(int caseContextArtifactID, int fieldArtifactID)
		{
			_baseServiceContext.AppArtifactID = caseContextArtifactID;
			return new FieldManagerImplementation().Read(_baseServiceContext, fieldArtifactID).ToField();
		}
	}
}