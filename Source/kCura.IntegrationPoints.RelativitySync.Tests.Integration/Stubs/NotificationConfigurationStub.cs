using System.Collections.Generic;
using Relativity.Sync.Configuration;

namespace kCura.IntegrationPoints.RelativitySync.Tests.Integration.Stubs
{
	internal sealed class NotificationConfigurationStub : INotificationConfiguration
	{
		public string JobStatus { get; set; }
		public bool SendEmails { get; set; }
		public IEnumerable<string> EmailRecipients { get; set; }
	}
}