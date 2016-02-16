using Relativity.API;

namespace kCura.IntegrationPoints.DocumentTransferProvider.Adaptors.Implementations
{
	public class ConfigAdapter : IIntegrationPointsConfig
	{
		private readonly IDBContext _context;

		public ConfigAdapter(IDBContext context)
		{
			_context = context;
		}

		public string GetWebApiUrl
		{
			// TODO: This is NOT an acceptable solution. We must look into using kCura.Config -- biedrzycki: Feb 16th, 2016
			get { return _context.ExecuteSqlStatementAsScalar<string>(" SELECT [Value] FROM eddsdbo.InstanceSetting Where InstanceSetting.Name = 'WebApiPath' and Section = 'kCura.IntegrationPoints' "); }
		}
	}
}