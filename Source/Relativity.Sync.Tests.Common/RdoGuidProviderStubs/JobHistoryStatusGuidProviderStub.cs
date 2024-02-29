using System;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Tests.Common.RdoGuidProviderStubs
{
    internal class JobHistoryStatusGuidProviderStub : IJobHistoryStatusProvider
    {
        public Guid CompletedGuid { get; set; }

        public Guid CompletedWithErrorsGuid { get; set; }

        public Guid JobFailedGuid { get; set; }

        public Guid ProcessingGuid { get; set; }

        public Guid StoppedGuid { get; set; }

        public Guid StoppingGuid { get; set; }

        public Guid SuspendedGuid { get; set; }

        public Guid SuspendingGuid { get; set; }

        public Guid ValidatingGuid { get; set; }

        public Guid ValidationFailedGuid { get; set; }
    }
}
