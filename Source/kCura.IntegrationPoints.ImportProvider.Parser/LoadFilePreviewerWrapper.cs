using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using kCura.WinEDDS;
using kCura.IntegrationPoints.ImportProvider.Parser.Interfaces;
using Relativity.Logging;

namespace kCura.IntegrationPoints.ImportProvider.Parser
{
	public class LoadFilePreviewerWrapper : ILoadFilePreviewer
	{
		private readonly LoadFilePreviewer _loadFilePreviewer;

		public LoadFilePreviewerWrapper(LoadFile loadFile, ILog logger, int timeZoneOffset, bool errorsOnly, bool doRetryLogic)
		{
			//[REL-838809]: Resolve correlationIdFunc
			_loadFilePreviewer = new LoadFilePreviewer(
	            args: loadFile, 
	            reporter: null, 
	            logger: logger,
	            timeZoneOffset: timeZoneOffset, 
	            errorsOnly: errorsOnly, 
	            doRetryLogic: doRetryLogic, 
	            tokenSource: new CancellationTokenSource(),
				correlationIdFunc : () => string.Empty);
		}

		public List<object> ReadFile(bool previewChoicesAndFolders)
		{
			int formType = 0;
			if (previewChoicesAndFolders)
			{
				formType = 2;
			}

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
