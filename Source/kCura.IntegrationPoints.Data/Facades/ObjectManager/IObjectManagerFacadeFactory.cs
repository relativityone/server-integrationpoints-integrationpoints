using Relativity.API;

namespace kCura.IntegrationPoints.Data.Facades.ObjectManager
{
    internal interface IObjectManagerFacadeFactory
    {
        IObjectManagerFacade Create(ExecutionIdentity executionIdentity);
    }
}
