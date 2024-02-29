using System.Collections.Generic;

namespace Relativity.Sync.SyncConfiguration.Options
{
    /// <summary>
    /// Represents email notification options.
    /// </summary>
    public class EmailNotificationsOptions
    {
        /// <summary>
        /// Specifies email notification recipients list.
        /// </summary>
        public List<string> Emails { get; set; }

        /// <summary>
        /// Creates new instance of <see cref="EmailNotificationsOptions"/> class.
        /// </summary>
        /// <param name="emails">Email notification recipients.</param>
        public EmailNotificationsOptions(List<string> emails)
        {
            Emails = emails;
        }
    }
}
