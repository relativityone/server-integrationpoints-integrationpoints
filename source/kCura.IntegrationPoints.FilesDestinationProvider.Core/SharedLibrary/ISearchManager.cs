using System;
using kCura.WinEDDS;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary
{
    public interface ISearchManager : WinEDDS.Service.Export.ISearchManager, IDisposable
    {
        ViewFieldInfo[] RetrieveAllExportableViewFields(int caseContextArtifactID, int artifactTypeID);
    }
}