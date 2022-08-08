using Relativity.IntegrationPoints.Tests.Functional.Web.Controls;
using Relativity.Testing.Framework.Web.Components;
using Relativity.Testing.Framework.Web.Triggers;

namespace Relativity.IntegrationPoints.Tests.Functional.Web.Components
{
    using _ = IntegrationPointViewPage;

    [WaitUntilHasClass("rwc-tabbable-category", Timeout = 60)]
    internal class SummaryPageGeneralTab : RwcTabbableCategory<_>
    {
        #region 1st column
        public RwcTextField<_> Name { get; private set; }

        public RwcRadioButtonGroupField<_> Overwrite { get; private set; }

        public RwcTextAreaField<_> ExportType { get; private set; }

        public RwcTextAreaField<_> SourceDetails { get; private set; }

        public RwcTextField<_> SourceWorkspace { get; private set; }

        public RwcTextField<_> SourceRelativityInstance { get; private set; }

        public RwcTextField<_> TransferedObject { get; private set; }

        public RwcTextField<_> DestinationWorkspace { get; private set; }

        public RwcTextField<_> DestinationFolder { get; private set; }

        public RwcTextField<_> MultiSelectOverlay { get; private set; }

        public RwcTextField<_> UseFolderPathInfo { get; private set; }

        public RwcTextField<_> MoveExistingDocs { get; private set; }

        public RwcTextField<_> ImagePrecedence { get; private set; }

        public RwcTextField<_> CopyFilesToRepository { get; private set; }

        #endregion

        #region 2nd column

        public RwcBooleanDropdownInputField<_> LogErrors { get; private set; }

        public RwcBooleanDropdownInputField<_> HasErrors { get; private set; }

        public RwcTextAreaField<_> EmailNotificationRecipients { get; private set; }

        public RwcIntField<_> TotalOfDocuments { get; private set; }

        public RwcTextField<_> TotalOfNatives { get; private set; }

        public RwcTextField<_> TotalOfImages { get; private set; }

        public RwcTextField<_> CreateSavedSearch { get; private set; }

        #endregion
    }
}
