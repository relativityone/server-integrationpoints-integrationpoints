using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Relativity.DataTransfer.MessageService;

namespace kCura.IntegrationPoints.Core.Monitoring
{
	public class TotalItemsMessage : Message
	{
		public TotalItemsMessage(string name, object correlationId, long value, string provider) : base(name, correlationId)
		{
			Provider = provider;
			Value = value;
		}

		public long Value { get; }

		public string Provider { get; }
	}
}
