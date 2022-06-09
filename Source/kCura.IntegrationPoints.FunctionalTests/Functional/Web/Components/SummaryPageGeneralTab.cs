using Relativity.Testing.Framework.Web.Components;
using Relativity.Testing.Framework.Web.Triggers;

namespace Relativity.IntegrationPoints.Tests.Functional.Web.Components
{
    using _ = IntegrationPointViewPage;

    [WaitUntilHasClass("rwc-tabbable-category", Timeout = 60)]
    internal class SummaryPageGeneralTab : RwcTabbableCategory<_>
    {
        #region 1st column
        public RwcTextInputField<_> Name { get; private set; }
        public RwcTextInputField<_> Overwrite { get; private set; }
        public RwcTextInputField<_> ExportType { get; private set; }
        public RwcTextInputField<_> SourceDetails { get; private set; }
        public RwcTextInputField<_> SourceWorkspace { get; private set; }
        public RwcTextInputField<_> SourceRelInstance { get; private set; }
        public RwcTextInputField<_> TransferredObject { get; private set; }
        public RwcTextInputField<_> DestinationWorkspace { get; private set; }
        public RwcTextInputField<_> DestinationFolder { get; private set; }
        public RwcTextInputField<_> MultiSelectOverlay { get; private set; }
        public RwcTextInputField<_> UseFolderPathInfo { get; private set; }
        public RwcTextInputField<_> MoveExistingDocs { get; private set; }
        public RwcTextInputField<_> ImagePrecedence { get; private set; }
        public RwcTextInputField<_> CopyFilesToRepository { get; private set; }
        #endregion

        #region 2nd column
        public RwcTextInputField<_> LogErrors { get; private set; }
        public RwcTextInputField<_> HasErrors { get; private set; }
        public RwcTextInputField<_> EmailNotificationRecipient { get; private set; }
        public RwcTextInputField<_> TotalDocuments { get; private set; }
        public RwcTextInputField<_> TotalNatives { get; private set; }
        public RwcTextInputField<_> TotalImages { get; private set; }
        public RwcTextInputField<_> CreateSavedSearch { get; private set; }
        #endregion
    }
}
