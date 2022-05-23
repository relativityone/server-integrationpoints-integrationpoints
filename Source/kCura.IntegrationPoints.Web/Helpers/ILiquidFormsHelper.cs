using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Web.Helpers
{
    public interface ILiquidFormsHelper
    {
        Task<bool> IsLiquidForms(int workspaceArtifactId);
    }
}