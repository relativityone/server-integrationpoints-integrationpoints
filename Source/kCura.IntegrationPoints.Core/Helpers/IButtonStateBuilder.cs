using System.Threading.Tasks;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Helpers
{
    public interface IButtonStateBuilder
    {
        Task<ButtonStateDTO> CreateButtonStateAsync(int applicationArtifactId, int integrationPointArtifactId);
    }
}
