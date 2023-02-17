using System.ComponentModel;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Helpers;
using kCura.WinEDDS;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Process
{
    public class ExportedObjectBuilder : IExportedObjectBuilder
    {
        private readonly IExportedArtifactNameRepository _nameRepository;

        public ExportedObjectBuilder(IExportedArtifactNameRepository nameRepository)
        {
            _nameRepository = nameRepository;
        }

        public void SetExportedObjectIdAndName(ExportSettings exportSettings, ExportFile exportFile)
        {
            exportFile.ExportNativesToFileNamedFrom = ParseNativesFilenameFromType(exportSettings.ExportNativesToFileNamedFrom);
            switch (exportSettings.TypeOfExport)
            {
                case ExportSettings.ExportType.SavedSearch:
                    exportFile.ArtifactID = exportSettings.SavedSearchArtifactId;
                    exportFile.LoadFilesPrefix = _nameRepository.GetSavedSearchName(exportSettings.WorkspaceId, exportSettings.SavedSearchArtifactId);
                    break;
                case ExportSettings.ExportType.Folder:
                case ExportSettings.ExportType.FolderAndSubfolders:
                    exportFile.ArtifactID = exportSettings.FolderArtifactId;
                    exportFile.ViewID = exportSettings.ViewId;
                    exportFile.LoadFilesPrefix = _nameRepository.GetViewName(exportSettings.WorkspaceId, exportSettings.ViewId);
                    break;
                case ExportSettings.ExportType.ProductionSet:
                    exportFile.ArtifactID = exportSettings.ProductionId;
                    exportFile.LoadFilesPrefix = _nameRepository.GetProductionName(exportSettings.WorkspaceId, exportSettings.ProductionId);
                    break;
                default:
                    throw new InvalidEnumArgumentException($"Unknown ExportSettings.ExportType ({exportSettings.TypeOfExport})");
            }
        }

        private static ExportNativeWithFilenameFrom ParseNativesFilenameFromType(ExportSettings.NativeFilenameFromType? exportSettingsExportNativesToFileNamedFrom)
        {
            if (!exportSettingsExportNativesToFileNamedFrom.HasValue)
            {
                // We can't return ExportNativeWithFilenameFrom.Select as this will couse issues in RDC Export code
                return ExportNativeWithFilenameFrom.Identifier;
            }
            switch (exportSettingsExportNativesToFileNamedFrom)
            {
                case ExportSettings.NativeFilenameFromType.Identifier:
                    return ExportNativeWithFilenameFrom.Identifier;
                case ExportSettings.NativeFilenameFromType.Production:
                    return ExportNativeWithFilenameFrom.Production;
                case ExportSettings.NativeFilenameFromType.Custom:
                    return ExportNativeWithFilenameFrom.Custom;
                default:
                    throw new InvalidEnumArgumentException($"Unknown ExportSettings.NativeFilenameFromType ({exportSettingsExportNativesToFileNamedFrom})");
            }
        }
    }
}
