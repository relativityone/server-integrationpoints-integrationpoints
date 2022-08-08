using System.Collections.Generic;

namespace kCura.IntegrationPoints.Data.Repositories
{
    public interface IRepository<T> where T : BaseRdo, new()
    {
        IEnumerable<T> GetAll();

        int Create(T rdo);

        bool Update(T rdo);

        bool Delete(T rdo);
    }
}
