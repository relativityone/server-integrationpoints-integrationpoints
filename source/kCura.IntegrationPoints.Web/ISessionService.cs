using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Core.Models;

namespace kCura.IntegrationPoints.Web
{
	public interface ISessionService
	{
		int WorkspaceID { get; }
		Dictionary<string, IEnumerable<FieldMap>> Fields { get; } 
	}
}
