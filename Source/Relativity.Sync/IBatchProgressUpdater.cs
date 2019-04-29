using Relativity.Sync.Storage;
using System.Threading.Tasks;

namespace Relativity.Sync
{
	internal interface IBatchProgressUpdater
	{
		Task UpdateProgressAsync(IBatch batch, int completedRecordsCount, int failedRecordsCount);
	}
}