using System.Collections.Generic;
using System.Threading.Tasks;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.Sync.Transfer
{
	internal interface ISpecialFieldBuilder
	{
		IEnumerable<FieldInfo> BuildColumns();

		Task<ISpecialFieldRowValuesBuilder> GetRowValuesBuilderAsync(int sourceWorkspaceArtifactId, RelativityObjectSlim[] documents);
	}

	internal interface ISpecialFieldRowValuesBuilder
	{
		IEnumerable<SpecialFieldType> AllowedSpecialFieldTypes { get; }
		object BuildRowValue(FieldInfo fieldInfo, RelativityObjectSlim document, object initialValue);
	}
}