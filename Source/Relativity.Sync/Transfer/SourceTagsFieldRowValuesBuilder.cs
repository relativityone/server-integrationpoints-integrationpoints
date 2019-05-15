using System.Collections.Generic;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.Sync.Transfer
{
	internal sealed class SourceTagsFieldRowValuesBuilder : ISpecialFieldRowValuesBuilder
	{
		public IEnumerable<SpecialFieldType> AllowedSpecialFieldTypes => new[] {SpecialFieldType.SourceJob, SpecialFieldType.SourceWorkspace};

		public IEnumerable<object> BuildRowValues(FieldInfo fieldInfo, RelativityObjectSlim document, object initialValue)
		{
			switch (fieldInfo.SpecialFieldType)
			{
				case SpecialFieldType.SourceJob:
				case SpecialFieldType.SourceWorkspace:
					yield return initialValue;
					break;
			}
		}
	}
}