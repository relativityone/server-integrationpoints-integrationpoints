using Atata;
using Relativity.Testing.Framework.Web.Components;

namespace Relativity.IntegrationPoints.Tests.Functional.Web.Components
{
    using _ = IntegrationPointViewPage;

    internal class JobHistoryStatusTab : RwcItemList<JobHistoryRow, _>
    {
    }

    internal class JobHistoryRow : RwaGridRow<_>
    {
        public Text<_> Name { get; private set; }

        public Text<_> JobType { get; private set; }

        public Text<_> JobStatus { get; private set; }

        public Text<_> ItemsTransferred { get; private set; }

        public Text<_> TotalItems { get; private set; }
    }
}
