using System.Threading.Tasks;

namespace Relativity.Sync.Telemetry
{
	internal sealed class JobStatisticsContainer : IJobStatisticsContainer
	{
		public long TotalBytesTransferred { get; set; }
		public Task<long> NativesBytesRequested { get; set; }
	}
}