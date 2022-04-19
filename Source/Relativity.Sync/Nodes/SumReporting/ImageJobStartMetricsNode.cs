using Relativity.API;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Nodes.SumReporting
{
	internal class ImageJobStartMetricsNode : SyncNode<IImageJobStartMetricsConfiguration>
	{
		public ImageJobStartMetricsNode(ICommand<IImageJobStartMetricsConfiguration> command, IAPILog logger) : base(command, logger)
		{
			Id = "Reporting image job start metrics";
			ParallelGroupName = "Multi node";
		}
	}
}
