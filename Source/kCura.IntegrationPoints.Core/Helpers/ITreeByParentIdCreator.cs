using System.Collections.Generic;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Helpers
{
    public interface ITreeByParentIdCreator<T>
    {
        JsTreeItemDTO Create(IEnumerable<T> nodes);
    }
}
