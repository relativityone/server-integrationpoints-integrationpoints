using kCura.IntegrationPoints.Core.Models;

namespace kCura.IntegrationPoints.RelativitySync
{
	internal interface ISendEmailWorker
	{
		void Execute(EmailMessage details, long jobId);
	}
}