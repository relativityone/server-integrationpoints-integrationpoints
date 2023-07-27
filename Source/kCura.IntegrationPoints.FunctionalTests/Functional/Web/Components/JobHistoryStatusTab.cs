using Atata;
using Relativity.Testing.Framework.Web.Components;

namespace Relativity.IntegrationPoints.Tests.Functional.Web.Components
{
    using _ = IntegrationPointViewPage;

    [FindByXPath("rwc-item-list", As = FindAs.ShadowHost)]
    [FindByXPath("rwc-grid", As = FindAs.ShadowHost)]
    [ControlDefinition("*")]
    internal class JobHistoryStatusTab : RwcGrid<JobHistoryRow, _>
    {
    }

    internal class JobHistoryRow : RwaGridRow<_>
    {
        [FindByColumnIndex(0)]
        public Content<string, _> JobID { get; private set; }

        [FindByColumnIndex(2)]
        public Content<string, _> ArtifactID { get; private set; }

        [FindByColumnIndex(3)]
        public Content<string, _> Name { get; private set; }

        [FindByColumnIndex(4)]
        public Content<string, _> JobType { get; private set; }

        [FindByColumnIndex(5)]
        public Content<string, _> JobStatus { get; private set; }

        [FindByColumnIndex(7)]
        public Content<string, _> ItemsRead { get; private set; }

        [FindByColumnIndex(8)]
        public Content<string, _> ItemsTransferred { get; private set; }

        [FindByColumnIndex(9)]
        public Content<string, _> TotalItems { get; private set; }

        [FindByColumnIndex(11)]
        public Content<string, _> SystemCreatedBy { get; private set; }
    }
}
