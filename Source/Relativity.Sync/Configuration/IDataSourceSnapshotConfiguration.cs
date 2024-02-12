﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Configuration
{
    internal interface IDataSourceSnapshotConfiguration : IConfiguration
    {
        int SourceWorkspaceArtifactId { get; }

        int DataSourceArtifactId { get; }

        IList<FieldMap> GetFieldMappings();

        bool IsSnapshotCreated { get; }

        Task SetSnapshotDataAsync(Guid runId, int totalRecordsCount);
    }
}
