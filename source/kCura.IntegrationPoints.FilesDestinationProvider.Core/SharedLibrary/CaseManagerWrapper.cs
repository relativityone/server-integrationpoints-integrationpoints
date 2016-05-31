using kCura.WinEDDS.Service;
using Relativity;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary
{
    internal class CaseManagerWrapper : ICaseManager
    {
        private readonly CaseManager _caseManager;

        public CaseManagerWrapper(CaseManager caseManager)
        {
            _caseManager = caseManager;
        }

        public void Dispose()
        {
            _caseManager.Dispose();
        }

        public CaseInfo Read(int caseArtifactID)
        {
            return _caseManager.Read(caseArtifactID);
        }
    }
}