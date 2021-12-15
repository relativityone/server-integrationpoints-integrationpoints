using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;

namespace Relativity.Sync.ExecutionConstrains
{
    internal sealed class NonDocumentSynchronizationExecutionConstrains : BaseSynchronizationExecutionConstrains
    {
        public NonDocumentSynchronizationExecutionConstrains(IBatchRepository batchRepository,ISyncLog syncLog) : base(batchRepository, syncLog)
        {
        }
    }
}
