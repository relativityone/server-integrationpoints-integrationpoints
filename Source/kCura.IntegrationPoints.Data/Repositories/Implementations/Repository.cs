using Relativity.Services.Objects.DataContracts;
using System.Collections.Generic;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
    public class Repository<T> : IRepository<T> where T : BaseRdo, new()
    {
        private readonly IRelativityObjectManager _objectManager;

        protected Repository(IRelativityObjectManager objectManager)
        {
            _objectManager = objectManager;
        }

        public IEnumerable<T> GetAll()
        {
            return _objectManager.Query<T>(new QueryRequest());
        }

        public int Create(T rdo)
        {
            return _objectManager.Create(rdo);
        }

        public bool Update(T rdo)
        {
            return _objectManager.Update(rdo);
        }

        public bool Delete(T rdo)
        {
            return _objectManager.Delete(rdo);
        }
    }
}
