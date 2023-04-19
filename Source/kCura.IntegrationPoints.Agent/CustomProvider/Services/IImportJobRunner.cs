using System.Threading.Tasks;
using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services
{
    public interface IImportJobRunner
    {
        Task RunJobAsync(Job job);
    }
}