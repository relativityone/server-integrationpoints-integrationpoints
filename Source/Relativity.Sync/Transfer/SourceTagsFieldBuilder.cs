using System.Collections.Generic;
using System.Threading.Tasks;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.Sync.Transfer
{
	internal sealed class SourceTagsFieldBuilder : ISpecialFieldBuilder
	{
		private const string _SOURCE_WORKSPACE_FIELD_NAME = "Relativity Source Case";
		private const string _SOURCE_JOB_FIELD_NAME = "Relativity Source Job";

		public IEnumerable<FieldInfoDto> BuildColumns()
		{
			yield return new FieldInfoDto {SpecialFieldType = SpecialFieldType.SourceWorkspace, DisplayName = _SOURCE_WORKSPACE_FIELD_NAME, IsDocumentField = false};
			yield return new FieldInfoDto {SpecialFieldType = SpecialFieldType.SourceJob, DisplayName = _SOURCE_JOB_FIELD_NAME, IsDocumentField = false};
		}

		public async Task<ISpecialFieldRowValuesBuilder> GetRowValuesBuilderAsync(int sourceWorkspaceArtifactId, IEnumerable<int> documentArtifactIds)
		{
			await Task.Yield();
			return new SourceTagsFieldRowValuesBuilder();
		}
	}
}