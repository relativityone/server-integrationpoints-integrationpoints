using System;
using System.ComponentModel;
using System.Linq;

namespace kCura.IntegrationPoints.Domain.Models
{
    public enum JsTreeItemIconEnum
    {
        [Description("jstree-root-folder")]
        Root = 0,

        [Description("jstree-folder")]
        Folder = 10,

        [Description("jstree-folder-search")]
        SavedSearchFolder = 20,

        [Description("jstree-search")]
        SavedSearch = 30,

        [Description("jstree-search-personal")]
        SavedSearchPersonal = 40
    }
}
