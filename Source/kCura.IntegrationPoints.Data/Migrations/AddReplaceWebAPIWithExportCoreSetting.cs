using kCura.IntegrationPoints.Data.DbContext;
using kCura.IntegrationPoints.Data.Resources;

namespace kCura.IntegrationPoints.Data.Migrations
{
    public class AddReplaceWebApiWithExportCoreSetting : IMigration
    {
        private readonly IEddsDBContext _eddsContext;

        public AddReplaceWebApiWithExportCoreSetting(IEddsDBContext eddsContext)
        {
            _eddsContext = eddsContext;
        }

        public void Execute()
        {
            var sql = Resource.AddReplaceWebAPIWithExportCoreSetting;
            _eddsContext.ExecuteNonQuerySQLStatement(sql);
        }
    }
}
