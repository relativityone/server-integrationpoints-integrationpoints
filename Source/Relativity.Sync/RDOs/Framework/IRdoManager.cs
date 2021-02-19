using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services.ArtifactGuid;
using Relativity.Services.Interfaces.Field;
using Relativity.Services.Interfaces.Field.Models;
using Relativity.Services.Interfaces.ObjectType;
using Relativity.Services.Interfaces.ObjectType.Models;
using Relativity.Services.Interfaces.Shared;
using Relativity.Services.Interfaces.Shared.Models;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.RDOs.Framework.Attributes;

namespace Relativity.Sync.RDOs.Framework
{
    internal interface IRdoManager
    {
        Task EnsureTypeExistsAsync<TRdo>(int workspaceId) where TRdo : IRdoType;

        Task<TRdo> GetAsync<TRdo>(int workspaceId, int artifactId, params Expression<Func<TRdo, object>>[] fields)
            where TRdo : IRdoType, new();

        Task CreateAsync<TRdo>(int workspaceId, TRdo rdo, int? parentObjectId = null) where TRdo : IRdoType;

        Task SetValuesAsync<TRdo>(int workspaceId, TRdo rdo, params Expression<Func<TRdo, object>>[] fields)
            where TRdo : IRdoType;
    }
}