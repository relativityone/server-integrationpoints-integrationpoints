using System.Collections.Generic;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary;
using kCura.WinEDDS;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Helpers.FileNaming
{
    public interface IFileNameProvidersDictionaryBuilder
    {
        IDictionary<ExportNativeWithFilenameFrom, IFileNameProvider> Build(ExportDataContext exportContext);
    }
}