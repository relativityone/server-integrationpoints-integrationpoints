namespace kCura.IntegrationPoints.Data.DbContext
{
    public interface IDbContextFactory
    {
        IWorkspaceDBContext CreateWorkspaceDbContext(int workspaceId);

        IEddsDBContext CreatedEDDSDbContext();
    }
}
