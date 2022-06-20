using Atata;
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
        
        [FindByAttribute("label", TermCase.Title, OuterXPath = ".//rwc-radio-button-group-field/self::", As = FindAs.ShadowHost)]
        [FindByXPath("div[@class = 'rwa-base-field view cell']")]
        public Content<string, _> Overwrite { get; private set; }

        [FindByAttribute("label", TermCase.Title, OuterXPath = ".//rwc-text-area-field/self::", As = FindAs.ShadowHost)]
        [FindByXPath("div[@class = 'rwa-base-field view cell']")]
        public Content<string, _> ExportType { get; private set; }

        [FindByAttribute("label", TermCase.Title, OuterXPath = ".//rwc-text-area-field/self::", As = FindAs.ShadowHost)]
        [FindByXPath("div[@class = 'rwa-base-field view cell']")]
        public Content<string, _> SourceDetails { get; private set; }

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

        [FindByAttribute("label", TermCase.Title, OuterXPath = ".//rwc-boolean-dropdown-input-field/self::", As = FindAs.ShadowHost)]
        [FindByXPath("div[@class = 'rwa-base-field view cell']")]
        public Content<string, _> LogErrors { get; private set; }

        [FindByAttribute("label", TermCase.Title, OuterXPath = ".//rwc-boolean-dropdown-input-field/self::", As = FindAs.ShadowHost)]
        [FindByXPath("div[@class = 'rwa-base-field view cell']")]
        public Content<string, _> HasErrors { get; private set; }

        [FindByAttribute("label", TermCase.Title, OuterXPath = ".//rwc-text-area-field/self::", As = FindAs.ShadowHost)]
        [FindByXPath("div[@class = 'rwa-base-field view cell']")]
        public Content<string, _> EmailNotificationRecipients { get; private set; }

        public RwcTextField<_> TotalOfDocuments { get; private set; }

        public RwcTextField<_> TotalOfNatives { get; private set; }

        public RwcTextField<_> TotalOfImages { get; private set; }

        public RwcTextField<_> CreateSavedSearch { get; private set; }
        #endregion
    }
}
