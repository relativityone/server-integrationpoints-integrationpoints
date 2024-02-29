using System;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Tests.Common.RdoGuidProviderStubs
{
    internal class JobHistoryErrorGuidsProviderStub : IJobHistoryErrorGuidsProvider
    {
        public Guid TypeGuid { get; set; }

        public Guid ErrorMessagesGuid { get; set; }

        public Guid ErrorStatusGuid { get; set; }

        public Guid ErrorTypeGuid { get; set; }

        public Guid NameGuid { get; set; }

        public Guid SourceUniqueIdGuid { get; set; }

        public Guid StackTraceGuid { get; set; }

        public Guid TimeStampGuid { get; set; }

        public Guid ItemLevelErrorGuid { get; set; }

        public Guid JobLevelErrorGuid { get; set; }

        public Guid JobHistoryRelationGuid { get; set; }

        public Guid NewStatusGuid { get; set; }
    }
}
