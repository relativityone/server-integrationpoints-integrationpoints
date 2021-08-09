using System;
using Relativity.Services.Objects.DataContracts;
using System.Collections.Generic;

namespace Relativity.Sync.Transfer
{
	internal interface IImageSpecialFieldRowValuesBuilder
	{
		IEnumerable<SpecialFieldType> AllowedSpecialFieldTypes { get; }

		IEnumerable<object> BuildRowsValues(FieldInfoDto fieldInfoDto, RelativityObjectSlim document, Func<RelativityObjectSlim, string> identifierFieldValueSelector);
	}
}
