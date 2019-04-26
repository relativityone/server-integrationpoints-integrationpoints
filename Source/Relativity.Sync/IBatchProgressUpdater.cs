using System.Threading.Tasks;

namespace Relativity.Sync
{
	internal interface IBatchProgressUpdater
	{
		Task UpdateProgressAsync(int completedRecordsCount, int failedRecordsCount);
	}
}