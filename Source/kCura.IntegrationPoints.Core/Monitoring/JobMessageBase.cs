using Relativity.DataTransfer.MessageService;

namespace kCura.IntegrationPoints.Core.Monitoring
{
	public abstract class JobMessageBase : IMessage
	{
		public string Provider { get; set; }
	}
}