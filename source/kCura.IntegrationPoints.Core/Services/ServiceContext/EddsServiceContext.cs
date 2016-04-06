using Relativity.API;

namespace kCura.IntegrationPoints.Core.Services.ServiceContext
{
	public class EddsServiceContext : IEddsServiceContext
	{
		private readonly IServiceContextHelper _helper;
		private int? _userId;
		private IDBContext _sqlContext;

		public EddsServiceContext(IServiceContextHelper helper)
		{
			_helper = helper;
		}

		public int UserID
		{
			get
			{
				if (!_userId.HasValue) _userId = _helper.GetEddsUserID();
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
					_sqlContext = _helper.GetDBContext(-1);
				}
				return _sqlContext;
			}
			set { _sqlContext = value; }
		}
	}
}
