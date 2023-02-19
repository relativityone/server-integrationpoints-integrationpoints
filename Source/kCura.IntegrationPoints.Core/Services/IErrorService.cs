using kCura.IntegrationPoints.Core.Models;

namespace kCura.IntegrationPoints.Core.Services
{
    public interface IErrorService
    {
        void Log(ErrorModel error);
    }
}
