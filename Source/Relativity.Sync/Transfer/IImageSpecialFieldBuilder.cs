using System.Collections.Generic;
using System.Threading.Tasks;

namespace Relativity.Sync.Transfer
{
    internal interface IImageSpecialFieldBuilder
    {
        IEnumerable<FieldInfoDto> BuildColumns();

        Task<IImageSpecialFieldRowValuesBuilder> GetRowValuesBuilderAsync(int sourceWorkspaceArtifactId, int[] documentArtifactIds);

    }
}
