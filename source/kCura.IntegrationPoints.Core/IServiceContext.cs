using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Core
{
	public  interface IServiceContext
	{
		int UserID { get; set; }
		int WorkspaceID { get; set; }
	}
}
