using System;
using System.Collections.Generic;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.Sync.Transfer
{
    internal interface IImageSpecialFieldRowValuesBuilder
    {
        IEnumerable<SpecialFieldType> AllowedSpecialFieldTypes { get; }

        IEnumerable<object> BuildRowsValues(FieldInfoDto fieldInfoDto, RelativityObjectSlim document, Func<RelativityObjectSlim, string> identifierFieldValueSelector);
    }
}
