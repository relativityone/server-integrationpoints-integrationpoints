using System;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Tests.Common.RdoGuidProviderStubs
{
    internal class JobHistoryErrorStatusGuidProviderStub : IJobHistoryErrorStatusGuidProvider
    {
        public Guid New { get; set; }
        public Guid Expired { get; set; }
        public Guid InProgress { get; set; }
        public Guid Retried { get; set; }
    }
}