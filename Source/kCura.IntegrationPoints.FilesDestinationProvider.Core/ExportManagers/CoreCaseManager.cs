using Relativity;
using Relativity.Core;
using Relativity.Core.DTO;
using Relativity.Core.Service;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.ExportManagers
{
	public class CoreCaseManager : WinEDDS.Service.Export.ICaseManager
	{
		private readonly BaseServiceContext _baseServiceContext;

		public CoreCaseManager(BaseServiceContext baseServiceContext)
		{
			_baseServiceContext = baseServiceContext;
		}

		public CaseInfo Read(int caseArtifactId)
		{
			ICaseManager manager = new CaseManager();
			Case @case = manager.Read(_baseServiceContext, caseArtifactId);
			return @case.ToCaseInfo();
		}

		public string[] GetAllDocumentFolderPathsForCase(int caseArtifactID)
		{
			var manager = new CaseManager();
			return manager.GetAllDocumentFolderPathsForCase(_baseServiceContext, caseArtifactID);
		}

		public void Dispose()
		{
			
		}
	}
}