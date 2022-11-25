using kCura.IntegrationPoints.Data.DbContext;

namespace kCura.IntegrationPoints.Core.Services.ServiceContext
{
    public interface IEddsServiceContext
    {
        int UserID { get; set; }

        IRipDBContext SqlContext { get; }
    }
}
