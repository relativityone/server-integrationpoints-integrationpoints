using Relativity.API;

namespace kCura.IntegrationPoints.Core.Services.ServiceContext
{
	public class EddsServiceContext : IEddsServiceContext
	{
		private int? _userID;
		public int UserID
		{
			get
			{
				if (!_userID.HasValue) _userID = helper.GetEddsUserID();
				return _userID.Value;
			}
			set { _userID = value; }
		}

		private IDBContext _sqlContext;
		public IDBContext SqlContext
		{
			get
			{
				if (_sqlContext == null) _sqlContext = helper.GetDBContext(-1);
				return _sqlContext;
			}
			set { _sqlContext = value; }
		}

		private IServiceContextHelper helper { get; set; }
		public EddsServiceContext(IServiceContextHelper helper)
		{
			this.helper = helper;
		}
	}
}
