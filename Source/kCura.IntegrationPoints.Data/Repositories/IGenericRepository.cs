namespace kCura.IntegrationPoints.Data.Repositories
{
    public interface IGenericRepository<T> where T : BaseRdo, new()
    {
        int Create(T rdo);

        bool Update(T rdo);

        bool Delete(T rdo);
    }
}
