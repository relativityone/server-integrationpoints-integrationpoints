using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Data
{
	public interface IResourceDbProvider
	{
		string GetSchemalessResourceDataBasePrepend(int workspaceId);
		string GetResourceDbPrepend(int workspaceId);
	}
}
