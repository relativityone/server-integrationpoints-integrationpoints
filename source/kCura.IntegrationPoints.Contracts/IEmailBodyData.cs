using System.Collections.Generic;
using kCura.IntegrationPoints.Contracts.Models;

namespace kCura.IntegrationPoints.Contracts.Provider
{
	/// <summary>
	/// Represents text used for importing into the notification email body.
	/// </summary>
	public interface IEmailBodyData
	{
		/// <summary>
		/// Retrieves text to be included in notification email body.
		/// </summary>
		/// <param name="fields">The fields requested from the provider/synchronizer.</param>
		/// <param name="options">The options on a source provider that a user has set.</param>
		string GetEmailBodyData(IEnumerable<FieldEntry> fields, string options);
	}
}
