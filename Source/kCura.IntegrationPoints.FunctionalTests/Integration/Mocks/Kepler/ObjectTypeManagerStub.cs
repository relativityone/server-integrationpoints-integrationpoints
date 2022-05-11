using System.Threading.Tasks;
using Moq;
using Relativity.Services.Interfaces.ObjectType;
using Relativity.Services.Interfaces.ObjectType.Models;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Kepler
{
    public class ObjectTypeManagerStub : KeplerStubBase<IObjectTypeManager>
    {
        public void SetupObjectTypeManager()
        {
        }
    }
}
