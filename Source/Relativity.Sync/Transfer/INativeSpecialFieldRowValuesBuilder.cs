using System.Collections.Generic;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.Sync.Transfer
{
    internal interface INativeSpecialFieldRowValuesBuilder
    {
        IEnumerable<SpecialFieldType> AllowedSpecialFieldTypes { get; }

        object BuildRowValue(FieldInfoDto fieldInfoDto, RelativityObjectSlim document, object initialValue);
    }
}
