using System;
using kCura.IntegrationPoints.Core.Managers;

namespace kCura.IntegrationPoints.Core.Factories
{
	public interface IIntegrationPointManagerFactory
	{
		IIntegrationPointManager CreateIntegrationPointManager(IContextContainer contextContainer);
	}
}
