using Relativity.Sync.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relativity.Sync.Nodes.SumReporting
{
	internal class NonDocumentJobStartMetricsNode : SyncNode<INonDocumentJobStartMetricsConfiguration>
	{
		public NonDocumentJobStartMetricsNode(ICommand<INonDocumentJobStartMetricsConfiguration> command, ISyncLog logger) : base(command, logger)
		{
			Id = "Reporting non-document job start metrics";
		}
	}
}
