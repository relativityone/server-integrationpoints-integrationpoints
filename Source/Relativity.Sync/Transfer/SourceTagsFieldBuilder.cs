using System.Collections.Generic;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Transfer
{
	internal sealed class SourceTagsFieldBuilder : ISpecialFieldBuilder
	{
		private readonly ISynchronizationConfiguration _configuration;

		public SourceTagsFieldBuilder(ISynchronizationConfiguration configuration)
		{
			_configuration = configuration;
		}

		public IEnumerable<FieldInfoDto> BuildColumns()
		{
			yield return FieldInfoDto.SourceWorkspaceField();
			yield return FieldInfoDto.SourceJobField();
		}

		public async Task<ISpecialFieldRowValuesBuilder> GetRowValuesBuilderAsync(int sourceWorkspaceArtifactId, ICollection<int> documentArtifactIds)
		{
			await Task.Yield();
			return new SourceTagsFieldRowValuesBuilder(_configuration);
		}
	}
}