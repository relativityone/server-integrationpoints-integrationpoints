using Atata;
using Relativity.Testing.Framework.Web.Triggers;
using Relativity.Testing.Framework.Web.Components;
using Relativity.IntegrationPoints.Tests.Functional.Web.ControlSearch;
using Relativity.IntegrationPoints.Tests.Functional.Web.Models;
using Relativity.IntegrationPoints.Tests.Functional.Web.Controls;
using Relativity.IntegrationPoints.Tests.Functional.Web.Interfaces;

namespace Relativity.IntegrationPoints.Tests.Functional.Web.Components
{
    using _ = ImportFromLoadFileConnectToSourcePage;

    [UseExternalFrame]
    [WaitUntilOverlayMissing(TriggerEvents.BeforeAccess, AbsenceTimeout = 120, AppliesTo = TriggerScope.Children)]
    [WaitForJQueryAjax(TriggerEvents.Init)]
    internal class ImportFromLoadFileConnectToSourcePage : WorkspacePage<_>, IHasTreeItems<_>
    {
        public Button<ImportFromLoadFileMapFieldsPage, _> Next { get; private set; }

        [FindById("configurationFrame")]
        public Frame<_> ConfigurationFrame { get; private set; }

        [FindByPrecedingDivContent]
        [WaitFor]
        [SwitchToFrame(nameof(ConfigurationFrame), TriggerEvents.BeforeAccess)]
        public Select2<IntegrationPointImportTypes, _> ImportType { get; private set; }

        [FindByPrecedingDivContent]
        [WaitFor]
        [SwitchToFrame(nameof(ConfigurationFrame), TriggerEvents.BeforeAccess)]
        public Control<_> WorkspaceDestinationFolder { get; set; }

        [FindByPrecedingDivContent]
        [SwitchToFrame(nameof(ConfigurationFrame), TriggerEvents.BeforeAccess)]
        [WaitForJQueryAjax(TriggerEvents.AfterClick)]
        public Control<_> ImportSource { get; private set; }

        [FindByPrecedingDivContent]
        [WaitFor]
        [SwitchToFrame(nameof(ConfigurationFrame), TriggerEvents.BeforeAccess)]
        public Select2<string,_> Column { get; set; }

        [FindByPrecedingDivContent]
        [WaitFor]
        [SwitchToFrame(nameof(ConfigurationFrame), TriggerEvents.BeforeAccess)]
        public Select2<string, _> Quote { get; set; }

        [FindByXPath("ul[contains(@class,'jstree-container-ul')]", Visibility = Visibility.Visible)]
        [SwitchToFrame(nameof(ConfigurationFrame), TriggerEvents.BeforeAccess)]
        public UnorderedList<TreeItemControl<_>, _> TreeItems { get; private set; }
    }
}
