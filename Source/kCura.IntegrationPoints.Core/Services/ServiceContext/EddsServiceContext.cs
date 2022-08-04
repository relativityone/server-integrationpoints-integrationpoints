using Relativity.API;

namespace kCura.IntegrationPoints.Core.Services.ServiceContext
{
    public class EddsServiceContext : IEddsServiceContext
    {
        private readonly IServiceContextHelper _serviceContextHelper;
        private int? _userId;
        private IDBContext _sqlContext;

        public EddsServiceContext(IServiceContextHelper serviceContextHelper)
        {
            _serviceContextHelper = serviceContextHelper;
        }

        public int UserID
        {
            get
            {
                if (!_userId.HasValue)
                {
                    _userId = _serviceContextHelper.GetEddsUserID();
                }
                return _userId.Value;
            }
            set { _userId = value; }
        }

        public IDBContext SqlContext
        {
            get
            {
                if (_sqlContext == null)
                {
                    _sqlContext = _serviceContextHelper.GetDBContext(-1);
                }
                return _sqlContext;
            }
        }
    }
}
