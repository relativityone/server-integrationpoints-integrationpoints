using System.Collections.Generic;
using System.Threading.Tasks;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.Sync.Transfer
{
	internal sealed class SourceTagsFieldBuilder : ISpecialFieldBuilder
	{
		private const string _SOURCE_WORKSPACE_FIELD_NAME = "Relativity Source Case";
		private const string _SOURCE_JOB_FIELD_NAME = "Relativity Source Job";

		public IEnumerable<FieldInfo> BuildColumns()
		{
			yield return new FieldInfo {SpecialFieldType = SpecialFieldType.SourceWorkspace, DisplayName = _SOURCE_WORKSPACE_FIELD_NAME, IsDocumentField = true};
			yield return new FieldInfo {SpecialFieldType = SpecialFieldType.SourceJob, DisplayName = _SOURCE_JOB_FIELD_NAME, IsDocumentField = true};
		}

		public async Task<ISpecialFieldRowValuesBuilder> GetRowValuesBuilderAsync(int sourceWorkspaceArtifactId, RelativityObjectSlim[] documents)
		{
			await Task.Yield();
			return new SourceTagsFieldRowValuesBuilder();
		}
	}
}