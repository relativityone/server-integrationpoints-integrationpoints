using Atata;
using Relativity.Testing.Framework.Web.Triggers;
using Relativity.Testing.Framework.Web.Attributes;
using Relativity.Testing.Framework.Web.Components;
using Relativity.IntegrationPoints.Tests.Functional.Web.Models;

namespace Relativity.IntegrationPoints.Tests.Functional.Web.Components
{
    using _ = IntegrationPointListPage;

    [PageName("Integration Points")]
    [UseExternalFrame]
    internal class IntegrationPointListPage : WorkspacePage<_>
    {
        public Button<IntegrationPointEditPage, _> NewIntegrationPoint { get; private set; }

        public RelativityGrid<IntegrationPointEdit, IntegrationPointRow, _> IntegrationPoint { get; private set; }

        public class IntegrationPointRow : RelativityGridRow<IntegrationPointEdit, _>
        {
            [FindByContent]
            public LinkDelegate<IntegrationPointEditPage, _> Edit { get; private set; }

            public LinkDelegate<IntegrationPointViewPage, _> Name { get; private set; }

            [Term("Name")]
            public LinkDelegate<IntegrationPointViewPage, _> View { get; private set; }
        }
    }
}