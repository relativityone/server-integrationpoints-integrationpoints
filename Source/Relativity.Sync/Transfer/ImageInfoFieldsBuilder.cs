using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Relativity.Sync.Transfer
{
	internal sealed class ImageInfoFieldsBuilder : ISpecialFieldBuilder
	{
		public IEnumerable<FieldInfoDto> BuildColumns()
		{
			yield return FieldInfoDto.ImageFileNameField();
			yield return FieldInfoDto.ImageFileLocationField();
		}

		public Task<ISpecialFieldRowValuesBuilder> GetRowValuesBuilderAsync(int sourceWorkspaceArtifactId, ICollection<int> documentArtifactIds)
		{
			return Task.FromResult<ISpecialFieldRowValuesBuilder>(new ImageInfoRowValuesBuilder());
		}
	}
}
