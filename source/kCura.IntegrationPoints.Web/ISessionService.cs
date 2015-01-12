using System.Collections.Generic;
using kCura.IntegrationPoints.Contracts.Models;

namespace kCura.IntegrationPoints.Web
{
	public interface ISessionService
	{
		int WorkspaceID { get; }
		int UserID { get; }
		Dictionary<string, IEnumerable<FieldMap>> Fields { get; } 
	}
}
