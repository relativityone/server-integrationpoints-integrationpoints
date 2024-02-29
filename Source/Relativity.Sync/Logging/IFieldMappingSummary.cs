using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Relativity.Sync.Logging
{
    internal interface IFieldMappingSummary
    {
        Task<Dictionary<string, object>> GetFieldsMappingSummaryAsync(CancellationToken token);
    }
}
