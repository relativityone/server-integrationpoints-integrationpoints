using kCura.IntegrationPoints.Domain.Extensions;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.Services.Folder;
using System.Collections.Generic;
using System.Linq;

namespace kCura.IntegrationPoints.Core.Helpers.Implementations
{
    public class FolderTreeBuilder : IFolderTreeBuilder
    {
        public JsTreeItemDTO CreateItemWithChildren(Folder folder, bool isRoot)
        {
            var folderDto = new JsTreeItemDTO()
            {
                Id = folder.ArtifactID.ToString(),
                Text = folder.Name,
                Icon = isRoot ? JsTreeItemIconEnum.Root.GetDescription() : JsTreeItemIconEnum.Folder.GetDescription(),
                IsDirectory = true,
                Children = new List<JsTreeItemDTO>()
            };

            foreach (Folder folderChild in folder.Children ?? Enumerable.Empty<Folder>())
            {
                var childDto = new JsTreeItemDTO()
                {
                    Id = folderChild.ArtifactID.ToString(),
                    Text = folderChild.Name,
                    Icon = JsTreeItemIconEnum.Folder.GetDescription(),
                    IsDirectory = true
                };
                folderDto.Children.Add(childDto);
            }

            return folderDto;
        }

        public JsTreeItemDTO CreateItemWithoutChildren(Folder folder)
        {
            var folderDto = new JsTreeItemDTO()
            {
                Id = folder.ArtifactID.ToString(),
                Text = folder.Name,
                Icon = JsTreeItemIconEnum.Folder.GetDescription(),
                IsDirectory = true,
                Children = new List<JsTreeItemDTO>()
            };
            return folderDto;
        }
    }
}
