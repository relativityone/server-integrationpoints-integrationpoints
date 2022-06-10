using System;
using Relativity.Sync.Configuration;
using Relativity.Testing.Framework.Models;

namespace Relativity.IntegrationPoints.Tests.Functional.Web.Components
{
    internal partial class IntegrationPointViewPage
    {
        public SummaryPageGeneralTab SummaryPageGeneralTab { get; private set; }

        #region 1st column

        public string GetName => SummaryPageGeneralTab.Name.Value;
        public string GetExportType => SummaryPageGeneralTab.ExportType.Value;
        public DocumentOverwriteMode GetOverwriteMode => ParseEnum<DocumentOverwriteMode>(SummaryPageGeneralTab.Overwrite.Value);
        public string GetSourceDetails => SummaryPageGeneralTab.SourceDetails.Value;
        public string GetSourceWorkspace => SummaryPageGeneralTab.SourceWorkspace.Value;
        public string GetSourceRelInstance => SummaryPageGeneralTab.SourceRelInstance.Value;
        public ArtifactType GetTransferredObject => ParseEnum<ArtifactType>(SummaryPageGeneralTab.TransferredObject.Value);
        public string GetDestinationWorkspace => SummaryPageGeneralTab.DestinationWorkspace.Value;
        public string GetDestinationFolder => SummaryPageGeneralTab.DestinationFolder.Value;
        public FieldOverlayBehavior GetMultiSelectOverlay => ParseEnum<FieldOverlayBehavior>(SummaryPageGeneralTab.MultiSelectOverlay.Value);
        public bool GetUseFolderPathInfo => Convert.ToBoolean(SummaryPageGeneralTab.UseFolderPathInfo.Value);
        public bool GetMoveExistingDocs => Convert.ToBoolean(SummaryPageGeneralTab.MoveExistingDocs.Value);
        public string GetImagePrecedence => SummaryPageGeneralTab.ImagePrecedence.Value;
        public bool GetCopyFilesToRepository => Convert.ToBoolean(SummaryPageGeneralTab.CopyFilesToRepository.Value);

        #endregion

        #region 2nd column

        public bool GetLogErrors => Convert.ToBoolean(SummaryPageGeneralTab.LogErrors.Value);
        public bool GetHasErrors => Convert.ToBoolean(SummaryPageGeneralTab.HasErrors.Value);
        public string GetEmailNotificationRecipient => SummaryPageGeneralTab.EmailNotificationRecipient.Value;
        public int GetTotalDocuments => Convert.ToInt32(SummaryPageGeneralTab.TotalDocuments.Value);
        public int GetTotalNatives => Convert.ToInt32(SummaryPageGeneralTab.TotalNatives.Value);
        public int GetTotalImages => Convert.ToInt32(SummaryPageGeneralTab.TotalImages.Value);
        public bool GetCreateSavedSearch => Convert.ToBoolean(SummaryPageGeneralTab.CreateSavedSearch.Value);

        #endregion

        private T ParseEnum<T>(string enumStringValue) where T : struct
        {
            bool isParsed = Enum.TryParse(enumStringValue,
                out T parsedEnum);

            if (!isParsed)
            {
                throw new FormatException($"Unable to parse {typeof(T)} with value {enumStringValue}");
            }

            return parsedEnum;
        }

    }
}
