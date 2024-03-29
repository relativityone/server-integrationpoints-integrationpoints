﻿using Relativity.Testing.Framework.Web.Models;

namespace Relativity.IntegrationPoints.Tests.Functional.Web.Models
{
    internal abstract class RelativityProviderConnectToSource
    {
        public string DestinationWorkspace { get; set; }

        public RelativityProviderDestinationLocations Location { get; set; }
    }

    // This looks kinda crazy, right? But there is method in this madness...
    // When calling ApplyModel the order of properties matters, so Source can't be on the base class because it would be applied after ProductionSet or SavedSearch, and if correct Source is not selected this will fail.
    internal class RelativityProviderConnectToSavedSearchSource : RelativityProviderConnectToSource
    {
        public RelativityProviderSources Source { get; } = RelativityProviderSources.SavedSearch;

        public string SavedSearch { get; set; }

        public YesNo TagDocumentsWithJobName { get; set; }
    }

    internal class RelativityProviderConnectToProductionSource : RelativityProviderConnectToSource
    {
        public RelativityProviderSources Source { get; } = RelativityProviderSources.Production;

        public string ProductionSet { get; set; }
    }

    internal class RelativityProviderConnectToViewSource
    {
        public string View { get; set; }

        public string DestinationWorkspace { get; set; }
    }
}
