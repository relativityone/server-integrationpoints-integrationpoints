using System.Collections.Generic;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Helpers
{
    public interface IDirectoryTreeCreator<T> where T : JsTreeItemBaseDTO
    {
        List<T> GetChildren(string path, bool isRoot, bool includeFiles = false);
    }
}