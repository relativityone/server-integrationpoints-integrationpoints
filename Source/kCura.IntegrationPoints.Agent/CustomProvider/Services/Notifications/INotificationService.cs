using System.Threading.Tasks;
using kCura.IntegrationPoints.Agent.CustomProvider.DTO;
using kCura.IntegrationPoints.Agent.CustomProvider.ImportStage;
using kCura.IntegrationPoints.Core.Models;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services.Notifications
{
    internal interface INotificationService
    {
        Task PrepareAndSendEmailNotificationAsync(ImportJobContext jobContext, string emails);
    }
}
