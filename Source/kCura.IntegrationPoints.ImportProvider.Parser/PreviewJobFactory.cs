using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.ImportProvider.Parser.Interfaces;

namespace kCura.IntegrationPoints.ImportProvider.Parser
{
    public class PreviewJobFactory : IPreviewJobFactory
    {
        private readonly IWinEddsLoadFileFactory _winEddsLoadFileFactory;
        public PreviewJobFactory(IWinEddsLoadFileFactory winEddsLoadFileFactory)
        {
            _winEddsLoadFileFactory = winEddsLoadFileFactory;
        }

        public IPreviewJob GetPreviewJob(ImportPreviewSettings settings)
        {
            var previewJob = new PreviewJob();
            previewJob.Init(_winEddsLoadFileFactory.GetLoadFile(settings), settings);
            return previewJob;
        }
    }
}
