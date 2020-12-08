using System.Collections.Generic;

namespace Relativity.Sync.SyncConfiguration.Options
{
	public class EmailNotificationsOptions
	{
		public List<string> Emails { get; set; }

		public EmailNotificationsOptions(List<string> emails)
		{
			Emails = emails;
		}
	}
}
