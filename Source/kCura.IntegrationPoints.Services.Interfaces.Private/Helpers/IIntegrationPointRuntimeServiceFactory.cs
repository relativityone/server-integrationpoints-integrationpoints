using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;

namespace kCura.IntegrationPoints.Services.Interfaces.Private.Helpers
{
	public interface IIntegrationPointRuntimeServiceFactory
	{
		IIntegrationPointService CreateIntegrationPointRuntimeService(Core.Models.IntegrationPointModel model);
	}
}
