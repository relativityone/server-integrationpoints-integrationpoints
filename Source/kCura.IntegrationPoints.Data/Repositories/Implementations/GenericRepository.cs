namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseRdo, new()
    {
        private readonly IRelativityObjectManager _objectManager;

        protected GenericRepository(IRelativityObjectManager objectManager)
        {
            _objectManager = objectManager;
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
