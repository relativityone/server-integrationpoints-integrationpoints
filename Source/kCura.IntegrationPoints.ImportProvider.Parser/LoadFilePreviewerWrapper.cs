using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.WinEDDS;
using kCura.IntegrationPoints.ImportProvider.Parser.Interfaces;

namespace kCura.IntegrationPoints.ImportProvider.Parser
{
    public class LoadFilePreviewerWrapper : ILoadFilePreviewer
    {
        private LoadFilePreviewer _loadFilePreviewer;

        public LoadFilePreviewerWrapper(LoadFile loadFile,int timeZoneOffset, bool errorsOnly, bool doRetryLogic)
        {
            _loadFilePreviewer = new LoadFilePreviewer(loadFile, timeZoneOffset, errorsOnly, doRetryLogic);
        }

        public List<object> ReadFile(bool previewChoicesAndFolders)
        {
            int formType = 0;
            if (previewChoicesAndFolders)
            {
                formType = 2;
            }

			List<object> result = new List<object>();
			ArrayList arrs = (ArrayList)_loadFilePreviewer.ReadFile(String.Empty, formType);

			return arrs.Cast<object>().ToList();
        }

        public void OnEventAdd(LoadFilePreviewer.OnEventEventHandler eventHandler)
        {
            _loadFilePreviewer.OnEvent += eventHandler;
        }

        public void OnEventRemove(LoadFilePreviewer.OnEventEventHandler eventHandler)
        {
            _loadFilePreviewer.OnEvent -= eventHandler;
        }
    }
}
