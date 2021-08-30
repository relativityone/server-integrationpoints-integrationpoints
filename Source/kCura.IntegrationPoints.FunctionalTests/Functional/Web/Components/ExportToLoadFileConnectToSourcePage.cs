﻿using Atata;
using Relativity.IntegrationPoints.Tests.Functional.Web.ControlSearch;
using Relativity.IntegrationPoints.Tests.Functional.Web.Models;
using Relativity.Testing.Framework.Web.Components;
using Relativity.Testing.Framework.Web.Triggers;

namespace Relativity.IntegrationPoints.Tests.Functional.Web.Components
{
    using _ = ExportToLoadFileConnectToSourcePage;
    
    [UseExternalFrame]
    [WaitUntilOverlayMissing(TriggerEvents.BeforeAccess, AbsenceTimeout = 30, AppliesTo = TriggerScope.Children)]
    [WaitForJQueryAjax(TriggerEvents.Init)]
    internal class ExportToLoadFileConnectToSourcePage : WorkspacePage<_>
    {
        public Button<ExportToLoadFileDestinationInformationPage, _> Next { get; private set; }
        
        [FindByPrecedingDivContent]
        public Select2<RelativityProviderSources, _> Source { get; private set; }

        [FindByPrecedingDivContent]
        public Select2<string, _> SavedSearch { get; private set; }

        [FindByPrecedingDivContent]
        public Input<int, _> StartExportAtRecord { get; private set; }
    }
}