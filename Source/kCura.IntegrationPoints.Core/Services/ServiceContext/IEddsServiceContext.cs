using Relativity.API;

namespace kCura.IntegrationPoints.Core.Services.ServiceContext
{
    public interface IEddsServiceContext
    {
        int UserID { get; set; }
        IDBContext SqlContext { get; }
    }
}
