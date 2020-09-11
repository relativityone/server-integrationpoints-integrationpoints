using Relativity.Services.Objects.DataContracts;
using System.Collections.Generic;

namespace Relativity.Sync.Transfer
{
	internal sealed class ImageInfoRowValuesBuilder : ISpecialFieldRowValuesBuilder
	{
		public IEnumerable<SpecialFieldType> AllowedSpecialFieldTypes => new []
		{
			SpecialFieldType.ImageFileName,
			SpecialFieldType.ImageFileLocation
		};

		public object BuildRowValue(FieldInfoDto fieldInfoDto, RelativityObjectSlim document, object initialValue)
		{
			return initialValue;
		}
	}
}
