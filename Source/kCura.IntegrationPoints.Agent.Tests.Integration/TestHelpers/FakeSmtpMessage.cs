using System.Collections.Generic;
using System.Linq;
using netDumbster.smtp;

namespace kCura.IntegrationPoints.Agent.Tests.Integration.TestHelpers
{
	internal class FakeSmtpMessage
	{
		public string FromAddress { get; }
		public IEnumerable<string> ToAddresses { get; }
		public string Subject { get; }
		public string Data { get; }

		public FakeSmtpMessage(SmtpMessage message)
		{
			FromAddress = message.FromAddress?.Address;
			ToAddresses = message.ToAddresses?.Select(x => x.Address);
			Subject = message.Headers["Subject"];
			Data = message.Data;
		}
	}
}
