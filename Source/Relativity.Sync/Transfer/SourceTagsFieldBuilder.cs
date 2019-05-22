using System.Collections.Generic;
using System.Threading.Tasks;

namespace Relativity.Sync.Transfer
{
	internal sealed class SourceTagsFieldBuilder : ISpecialFieldBuilder
	{
		public IEnumerable<FieldInfoDto> BuildColumns()
		{
			yield return FieldInfoDto.SourceWorkspaceField();
			yield return FieldInfoDto.SourceJobField();
		}

		public async Task<ISpecialFieldRowValuesBuilder> GetRowValuesBuilderAsync(int sourceWorkspaceArtifactId, ICollection<int> documentArtifactIds)
		{
			await Task.Yield();
			return new SourceTagsFieldRowValuesBuilder();
		}
	}
}