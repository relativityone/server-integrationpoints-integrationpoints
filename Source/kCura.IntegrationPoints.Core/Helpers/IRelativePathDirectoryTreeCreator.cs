using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Helpers
{
    public interface IRelativePathDirectoryTreeCreator<T> where T : JsTreeItemBaseDTO
    {
        List<T> GetChildren(string relativePath, bool isRoot, int wkspId, Guid integrationPointTypeIdentifier, bool includeFiles = false);
    }
}
