using kCura.IntegrationPoints.Domain.Models;
using Relativity.Services.Folder;

namespace kCura.IntegrationPoints.Core.Helpers
{
    public interface IFolderTreeBuilder
    {
        JsTreeItemDTO CreateItemWithChildren(Folder folder, bool isRoot);
        JsTreeItemDTO CreateItemWithoutChildren(Folder folder);
    }
}
