using kCura.IntegrationPoints.Core.Models;

namespace kCura.IntegrationPoints.RelativitySync.RipOverride
{
	internal interface ISendEmailWorker
	{
		void Execute(EmailJobParameters details, long jobId);
	}
}