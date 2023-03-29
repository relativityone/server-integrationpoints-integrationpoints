using System;

namespace Relativity.IntegrationPoints.Tests.Integration
{
    public class InstanceSettings
    {
        public string FriendlyInstanceName { get; set; }

        public string AllowNoSnapshotImport { get; set; }

        public string RestrictReferentialFileLinksOnImport { get; set; }

        public string MaximumNumberOfCharactersSupportedByLongText { get; set; }

        public string BlockedHosts { get; set; }

        public TimeSpan DrainStopTimeout { get; set; }

        public int IApiBatchSize { get; set; }

        public int CustomProviderBatchSize { get; set; }
    }
}
