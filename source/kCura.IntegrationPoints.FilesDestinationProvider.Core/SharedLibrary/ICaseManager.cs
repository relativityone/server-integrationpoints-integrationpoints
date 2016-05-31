using System;
using Relativity;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary
{
    public interface ICaseManager : IDisposable
    {
        CaseInfo Read(int caseArtifactID);
    }
}