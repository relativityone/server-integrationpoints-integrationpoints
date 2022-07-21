using System.Collections.Generic;

namespace Relativity.Sync.Storage
{
    internal interface IFieldMappings
    {
        IList<FieldMap> GetFieldMappings();
    }
}