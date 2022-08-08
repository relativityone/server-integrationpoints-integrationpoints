using System.Threading.Tasks;
using Moq;
using Relativity.Services.Error;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Kepler
{
    public class ErrorManagerStub : KeplerStubBase<IErrorManager>
    {
        public void SetupErrorManager()
        {
            Mock.Setup(x => x.CreateSingleAsync(It.IsAny<Error>()))
                .Returns((Error error) =>
                {
                    error.ArtifactID = ArtifactProvider.NextId();
                    Relativity.Errors.Add(error);
                    
                    return Task.FromResult(error.ArtifactID);
                });
        }
    }
}