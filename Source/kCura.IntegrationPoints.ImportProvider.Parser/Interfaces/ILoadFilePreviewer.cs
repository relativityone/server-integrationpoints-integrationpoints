using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.WinEDDS;

namespace kCura.IntegrationPoints.ImportProvider.Parser.Interfaces
{
    public interface ILoadFilePreviewer
    {
        List<object> ReadFile(bool previewChoicesAndFolders);

        void OnEventAdd(LoadFilePreviewer.OnEventEventHandler eventHandler);

        void OnEventRemove(LoadFilePreviewer.OnEventEventHandler eventHandler);

    }
}
