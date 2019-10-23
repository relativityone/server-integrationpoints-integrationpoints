using System.Collections.Generic;
using Relativity.IntegrationPoints.Contracts.Models;

namespace Relativity.IntegrationPoints.Contracts.Internals
{
	/// <summary>
	/// Represents text used for importing into the notification email body.
	/// This type is only for internal use in Integration Points.
	/// </summary>
	/// 
	/// This class is public because we do not want to add 'Internals Visible To' to many projects
	/// since it would force us to generate new sdk every time we would want to use it in new project.
	/// https://einstein.kcura.com/display/DV/Internal+classes+in+RIP+SDK
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
