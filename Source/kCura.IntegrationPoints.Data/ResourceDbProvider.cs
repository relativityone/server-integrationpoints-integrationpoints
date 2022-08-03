using Relativity.API;

namespace kCura.IntegrationPoints.Data
{
    public class ResourceDbProvider : IResourceDbProvider
    {
        private readonly IHelper _helper;

        public ResourceDbProvider(IHelper helper)
        {
            _helper = helper;
        }

        public string GetSchemalessResourceDataBasePrepend(int workspaceID)
        {
            IDBContext dbContext = _helper.GetDBContext(workspaceID);
            return _helper.GetSchemalessResourceDataBasePrepend(dbContext);
        }

        public string GetResourceDbPrepend(int workspaceID)
        {
            IDBContext dbContext = _helper.GetDBContext(workspaceID);
            return _helper.ResourceDBPrepend(dbContext);
        }
    }
}
