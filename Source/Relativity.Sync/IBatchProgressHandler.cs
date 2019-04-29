using kCura.Relativity.DataReaderClient;

namespace Relativity.Sync
{
	internal interface IBatchProgressHandler
	{
		void HandleProcessProgress(FullStatus status);

		void HandleProcessComplete(JobReport jobReport);
	}
}