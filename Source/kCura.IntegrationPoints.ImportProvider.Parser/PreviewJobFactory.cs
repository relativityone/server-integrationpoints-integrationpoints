using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.ImportProvider.Parser;
using kCura.IntegrationPoints.ImportProvider.Parser.Interfaces;

namespace kCura.IntegrationPoints.ImportProvider.Parser
{
	public class PreviewJobFactory : IPreviewJobFactory
	{
		IWinEddsLoadFileFactory _winEddsLoadFileFactory;
		public PreviewJobFactory(IWinEddsLoadFileFactory winEddsLoadFileFactory)
		{
			_winEddsLoadFileFactory = winEddsLoadFileFactory;
		}

		public IPreviewJob GetPreviewJob(ImportPreviewSettings settings)
		{
			PreviewJob previewJob = new PreviewJob();
			previewJob.Init(_winEddsLoadFileFactory.GetLoadFile(settings), settings);
			return previewJob;
		}
	}
}
