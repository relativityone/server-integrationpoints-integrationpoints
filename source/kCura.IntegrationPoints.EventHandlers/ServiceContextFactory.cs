using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Core;
using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers
{
	public class ServiceContextFactory
	{
		public static IServiceContext CreateServiceContext(IEHHelper helper)
		{
			return new ServiceContext
		 {
			 SqlContext = helper.GetDBContext(helper.GetActiveCaseID()),
			 WorkspaceID = helper.GetActiveCaseID()
		 };
		}
	}
}
