using System;
using Relativity.IntegrationPoints.Tests.Functional.Web.Models;
using Relativity.Sync.Configuration;
using Relativity.Testing.Framework.Web.Models;

namespace Relativity.IntegrationPoints.Tests.Functional.Web.Components
{
    internal partial class IntegrationPointViewPage
    {
        public SummaryPageGeneralTab SummaryPageGeneralTab { get; private set; }

        #region 1st column

        public string GetName() => SummaryPageGeneralTab.Name.Value;
        public string GetExportType() => SummaryPageGeneralTab.ExportType.Value;
        public RelativityProviderOverwrite GetOverwriteMode() => ParseEnum<RelativityProviderOverwrite>(SummaryPageGeneralTab.Overwrite.Value);
        public string GetSourceDetails() => SummaryPageGeneralTab.SourceDetails.Value;
        public string GetSourceWorkspaceName() => SummaryPageGeneralTab.SourceWorkspace.Value;
        public string GetSourceRelativityInstance() => SummaryPageGeneralTab.SourceRelativityInstance.Value;
        public ArtifactType GetTransferredObject() => ParseEnum<ArtifactType>(SummaryPageGeneralTab.TransferedObject.Value);
        public string GetDestinationWorkspaceName() => SummaryPageGeneralTab.DestinationWorkspace.Value;
        public string GetDestinationFolderName() => SummaryPageGeneralTab.DestinationFolder.Value;
        public FieldOverlayBehavior GetMultiSelectOverlayMode() => ParseEnum<FieldOverlayBehavior>(SummaryPageGeneralTab.MultiSelectOverlay.Value);
        public RelativityProviderFolderPathInformation GetUseFolderPathInfo() => ParseEnum<RelativityProviderFolderPathInformation>(SummaryPageGeneralTab.UseFolderPathInfo.Value);
        public YesNo GetMoveExistingDocs() => ParseEnum<YesNo>(SummaryPageGeneralTab.MoveExistingDocs.Value);
        public string GetImagePrecedence() => SummaryPageGeneralTab.ImagePrecedence.Value;
        public YesNo GetCopyFilesToRepository() => ParseEnum<YesNo>(SummaryPageGeneralTab.CopyFilesToRepository.Value);

        #endregion

        #region 2nd column

        public YesNo GetLogErrors() => ParseEnum<YesNo>(SummaryPageGeneralTab.LogErrors.Value);
        public YesNo GetHasErrors() => ParseEnum<YesNo>(SummaryPageGeneralTab.HasErrors.Value);
        public string GetEmailNotificationRecipients() => SummaryPageGeneralTab.EmailNotificationRecipients.Value;
        public int GetTotalDocuments() => Convert.ToInt32(SummaryPageGeneralTab.TotalOfDocuments.Value);
        public string GetTotalNatives() => SummaryPageGeneralTab.TotalOfNatives.Value;
        public string GetTotalImages() => SummaryPageGeneralTab.TotalOfImages.Value;
        public YesNo GetCreateSavedSearch() => ParseEnum<YesNo>(SummaryPageGeneralTab.CreateSavedSearch.Value);

        #endregion

        private T ParseEnum<T>(string enumStringValue) where T : struct
        {
            bool isParsed = Enum.TryParse(enumStringValue.Replace(" ", ""),
                out T parsedEnum);

            if (!isParsed)
            {
                throw new FormatException($"Unable to parse {typeof(T)} with value {enumStringValue}");
            }

            return parsedEnum;
        }

    }
}
