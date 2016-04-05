using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Core.Managers
{
	public interface IDestinationWorkspaceManager
	{
		/// <summary>
		/// Controls the CRUD operations of the Destination Workspace RDO
		/// </summary>
		void Execute();
	}
}
