using System.Collections.Generic;

namespace Relativity.Sync.Configuration
{
	internal interface INotificationConfiguration : IConfiguration
	{
		/// <summary>
		///     string will be changed after we introduce final progress handling
		/// </summary>
		string JobStatus { get; }

		bool SendEmails { get; }

		IEnumerable<string> EmailRecipients { get; }
	}
}