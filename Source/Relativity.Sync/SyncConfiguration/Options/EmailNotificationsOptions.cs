using System.Collections.Generic;

namespace Relativity.Sync.SyncConfiguration.Options
{
	/// <summary>
	/// 
	/// </summary>
	public class EmailNotificationsOptions
	{
		/// <summary>
		/// 
		/// </summary>
		public List<string> Emails { get; set; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="emails"></param>
		public EmailNotificationsOptions(List<string> emails)
		{
			Emails = emails;
		}
	}
}
