using System.Linq;
using System.Threading.Tasks;
using Moq;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Services.View;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Kepler
{
    public class ViewManagerStub : KeplerStubBase<IViewManager>
    {
        public void SetupViewManagerStub()
        {
            Mock.Setup(x => x.ReadSingleAsync(It.IsAny<int>(), It.IsAny<int>()))
                .Returns((int workspaceId, int viewId) =>
                {
                    ViewTest view = Relativity.Workspaces.First(x => x.ArtifactId == workspaceId).Views.SingleOrDefault(x => x.ArtifactId == viewId);

                    if (view == null)
                    {
                        return Task.FromResult(new View());
                    }

                    return Task.FromResult(view.ToView());
                });
        }
    }
}
