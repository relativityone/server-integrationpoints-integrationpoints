using Atata;
using Relativity.Testing.Framework.Web.Models;
using Relativity.Testing.Framework.Web.Triggers;
using Relativity.Testing.Framework.Web.Components;
using Relativity.IntegrationPoints.Tests.Functional.Web.Models;
using Relativity.IntegrationPoints.Tests.Functional.Web.ControlSearch;

namespace Relativity.IntegrationPoints.Tests.Functional.Web.Components
{
    using _ = ImportFromLoadFileMapFieldsPage;

    [UseExternalFrame]
    [WaitUntilOverlayMissing(TriggerEvents.BeforeAccess, PresenceTimeout = 4, AbsenceTimeout = 30, AppliesTo = TriggerScope.Children)]
    [WaitForJQueryAjax(TriggerEvents.Init)]
    internal class ImportFromLoadFileMapFieldsPage : WorkspacePage<_>
    {
        public Button<IntegrationPointViewPage, _> Save { get; private set; }

        [WaitFor]
        public Button<_> MapAllFields { get; private set; }

        [FindByPrecedingDivContent]
        public Select2<RelativityProviderOverwrite, _> Overwrite { get; private set; }

        [FindByPrecedingDivContent]
        public RadioButtonList<RelativityProviderCopyNativeFiles, _> CopyNativeFiles { get; private set; }

        [FindByPrecedingDivContent]
        [WaitForElement(WaitBy.Class, "field-label", Until.Visible)]
        public Select2<string, _> NativeFilePath { get; private set; }

        [FindByPrecedingDivContent]
        public RadioButtonList<YesNo, _> UseFolderPathInformation { get; private set; }

        [FindByPrecedingDivContent]
        [WaitForElement(WaitBy.Class, "field-label", Until.Visible)]
        public Select2<string, _> FolderPathInformation { get; private set; }
    }
}
