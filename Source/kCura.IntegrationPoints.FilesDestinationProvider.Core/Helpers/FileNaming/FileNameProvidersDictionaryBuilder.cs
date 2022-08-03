using System.Collections.Generic;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Extensions;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary;
using kCura.WinEDDS;
using kCura.WinEDDS.FileNaming.CustomFileNaming;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Helpers.FileNaming
{
    public class FileNameProvidersDictionaryBuilder : IFileNameProvidersDictionaryBuilder
    {
        public IDictionary<ExportNativeWithFilenameFrom, IFileNameProvider> Build(ExportDataContext exportContext)
        {
            bool nameTextAndNativesAfterBegBates = exportContext.ExportFile.AreSettingsApplicableForProdBegBatesNameCheck();
            var fileNameProviders = new Dictionary<ExportNativeWithFilenameFrom, IFileNameProvider>
            {
                [ExportNativeWithFilenameFrom.Identifier] = new IdentifierExportFileNameProvider(exportContext.ExportFile) ,
                [ExportNativeWithFilenameFrom.Production] = new ProductionExportFileNameProvider(exportContext.ExportFile, nameTextAndNativesAfterBegBates) ,
                [ExportNativeWithFilenameFrom.Custom] = new CustomFileNameProvider(
                    exportContext.Settings.FileNameParts, 
                    fileNamePartNameContainer: new FileNamePartProviderContainer(),
                    appendOriginalFileName: false)
            };

            return fileNameProviders;
        }
    }
}