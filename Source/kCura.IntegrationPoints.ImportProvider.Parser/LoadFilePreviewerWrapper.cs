using System;
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

        public object ReadFile()
        {
            return _loadFilePreviewer.ReadFile("", 0);
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
