using Relativity.Services.Objects.DataContracts;
using System.Collections.Generic;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
    public class Repository<T> : IRepository<T> where T : BaseRdo, new()
    {
        private readonly IRelativityObjectManager _objectManager;

        protected Repository(IRelativityObjectManager objectManager)
        {
            _objectManager = objectManager;
        }

        public IEnumerable<T> GetAll(ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser)
        {
            return _objectManager.Query<T>(new QueryRequest(), executionIdentity);
        }

        public int Create(T rdo, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser)
        {
            return _objectManager.Create(rdo, executionIdentity: executionIdentity);
        }

        public bool Update(T rdo, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser)
        {
            return _objectManager.Update(rdo, executionIdentity: executionIdentity);
        }

        public bool Delete(T rdo, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser)
        {
            return _objectManager.Delete(rdo, executionIdentity: executionIdentity);
        }
    }
}
