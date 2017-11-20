using System;
using System.Linq;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Services
{
    public interface ISavedSearchesTreeService
    {
        JsTreeItemDTO GetSavedSearchesTree(int workspaceArtifactId);
    }
}