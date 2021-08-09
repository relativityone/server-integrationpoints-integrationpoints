using System.Collections.Generic;
using System.Threading.Tasks;

namespace Relativity.Sync.Transfer
{
	internal interface INativeSpecialFieldBuilder
	{
        IEnumerable<FieldInfoDto> BuildColumns();

        Task<INativeSpecialFieldRowValuesBuilder> GetRowValuesBuilderAsync(int sourceWorkspaceArtifactId, int[] documentArtifactIds);
    }
}