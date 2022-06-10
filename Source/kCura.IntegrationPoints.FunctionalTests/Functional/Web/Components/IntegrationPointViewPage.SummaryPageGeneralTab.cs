using System;

namespace Relativity.IntegrationPoints.Tests.Functional.Web.Components
{
    internal partial class IntegrationPointViewPage
    {
        public SummaryPageGeneralTab SummaryPageGeneralTab { get; private set; }

        public string GetName => SummaryPageGeneralTab.Name.Value;
        public string GetExportType => SummaryPageGeneralTab.ExportType.Value;
        public string GetSourceDetails => SummaryPageGeneralTab.SourceDetails.Value;
        public string GetSourceRelInstance => SummaryPageGeneralTab.SourceRelInstance.Value;
        public string GetTransferredObject => SummaryPageGeneralTab.TransferredObject.Value;
        public string GetDestinationWorkspace => SummaryPageGeneralTab.DestinationWorkspace.Value;
        public string GetDestinationFolder => SummaryPageGeneralTab.DestinationFolder.Value;
        public string GetMultiSelectOverlay => SummaryPageGeneralTab.MultiSelectOverlay.Value;
        public string GetUseFolderPathInfo => SummaryPageGeneralTab.UseFolderPathInfo.Value;
        public string GetMoveExistingDocs => SummaryPageGeneralTab.MoveExistingDocs.Value;
        public string GetImagePrecedence => SummaryPageGeneralTab.ImagePrecedence.Value;
        public string GetCopyFilesToRepository => SummaryPageGeneralTab.CopyFilesToRepository.Value;

        public ImportOverwriteType GetOverwriteMode()
        {
            bool overwriteTypeParsed = ImportOverwriteType.TryParse(SummaryPageGeneralTab.Overwrite.Value, 
                out ImportOverwriteType overwriteType);
            
            if (!overwriteTypeParsed)
            {
                throw new  FormatException("Unable to Parse ImportOverwriteType");
            }

            return overwriteType;

        } 

    }
}
