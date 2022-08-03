using Relativity.API.Foundation.Repositories;

namespace kCura.IntegrationPoints.Data.Repositories
{
    public interface IFoundationRepositoryFactory
    {
        T GetRepository<T>(int workspaceID) where T : IRepository;
    }
}