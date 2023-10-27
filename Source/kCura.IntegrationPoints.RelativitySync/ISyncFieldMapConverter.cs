using System.Collections.Generic;
using Relativity.IntegrationPoints.FieldsMapping.Models;

using SyncFieldEntry = Relativity.Sync.Storage.FieldEntry;
using SyncFieldMap = Relativity.Sync.Storage.FieldMap;

namespace kCura.IntegrationPoints.RelativitySync
{
    public interface ISyncFieldMapConverter
    {
        List<SyncFieldMap> ConvertToSyncFieldMap(List<FieldMap> fieldsMapping);
    }
}
