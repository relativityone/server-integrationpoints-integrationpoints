using System.Collections;

namespace Relativity.Sync
{
	internal interface IJobProgressHandler
	{
		void HandleItemProcessed(long item);
		void HandleItemError(IDictionary row);
		void HandleProcessComplete(JobReport jobReport);
		void HandleFatalException(JobReport jobReport);
	}
}