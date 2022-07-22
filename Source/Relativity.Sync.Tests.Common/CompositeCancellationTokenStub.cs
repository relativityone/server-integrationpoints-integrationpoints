using Relativity.Sync.Logging;
using System;
using System.Threading;

namespace Relativity.Sync.Tests.Common
{
    internal class CompositeCancellationTokenStub : CompositeCancellationToken
    {
        public override bool IsStopRequested => IsStopRequestedFunc?.Invoke() ?? base.IsStopRequested;

        public override bool IsDrainStopRequested => IsDrainStopRequestedFunc?.Invoke() ?? base.IsDrainStopRequested;

        public Func<bool> IsStopRequestedFunc { get; set; }

        public Func<bool> IsDrainStopRequestedFunc { get; set; }

        public CompositeCancellationTokenStub() : base(CancellationToken.None, CancellationToken.None, new EmptyLogger())
        {
        }
    }
}
