using System.Collections.Generic;

namespace kCura.IntegrationPoints.Data.Migrations
{
    public class MigrationRunner
    {
        private readonly IEddsDBContext _eddsContext;
        private readonly IWorkspaceDBContext _workspaceContext;
        public MigrationRunner(IEddsDBContext eddsContext,IWorkspaceDBContext workspaceContext)
        {
            _eddsContext = eddsContext;
            _workspaceContext = workspaceContext;
        }
        public void Run()
        {
            foreach (var migration in GetMigrations())
            {
                migration.Execute();
            }
        }

        public virtual IEnumerable<IMigration> GetMigrations()
        {
            yield return new AddWebApiConfigValueMigration(_eddsContext);
            yield return new UpdateJobErrorsBlankToNo(_workspaceContext);
            yield return new AddReplaceWebApiWithExportCoreSetting(_eddsContext);
        }

    }
}