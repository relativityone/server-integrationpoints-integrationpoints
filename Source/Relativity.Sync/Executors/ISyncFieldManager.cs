using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Relativity.Services.Interfaces.Field.Models;

namespace Relativity.Sync.Executors
{
    internal interface ISyncFieldManager
    {
        Task EnsureFieldsExistAsync(int workspaceArtifactId, IDictionary<Guid, BaseFieldRequest> fieldRequests);
    }
}
