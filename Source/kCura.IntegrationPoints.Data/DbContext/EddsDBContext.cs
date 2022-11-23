using kCura.IntegrationPoints.Common;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.DbContext
{
    public class EddsDBContext : RipDbContextBase, IEddsDBContext
    {
        public EddsDBContext(IDBContext context, IRetryHandlerFactory retryHandlerFactory)
            : base(context, retryHandlerFactory)
        {
        }
    }
}
