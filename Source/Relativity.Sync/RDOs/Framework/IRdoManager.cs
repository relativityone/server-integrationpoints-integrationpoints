using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Relativity.Sync.RDOs.Framework
{
    internal interface IRdoManager
    {
        Task EnsureTypeExistsAsync<TRdo>(int workspaceId) where TRdo : IRdoType;

        Task<TRdo> GetAsync<TRdo>(int workspaceId, int artifactId, params Expression<Func<TRdo, object>>[] fields)
            where TRdo : IRdoType, new();

        Task CreateAsync<TRdo>(int workspaceId, TRdo rdo, int? parentObjectId = null) where TRdo : IRdoType;

        Task SetValuesAsync<TRdo>(int workspaceId, TRdo rdo)
            where TRdo : IRdoType;

        Task SetValueAsync<TRdo, TValue>(int workspaceId, TRdo rdo, Expression<Func<TRdo, TValue>> expression,
            TValue value) where TRdo : IRdoType;
    }
}
