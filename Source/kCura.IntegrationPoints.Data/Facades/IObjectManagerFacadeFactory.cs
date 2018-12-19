using Relativity.API;

namespace kCura.IntegrationPoints.Data.Facades
{
	internal interface IObjectManagerFacadeFactory
	{
		IObjectManagerFacade Create(ExecutionIdentity executionIdentity);
	}
}
