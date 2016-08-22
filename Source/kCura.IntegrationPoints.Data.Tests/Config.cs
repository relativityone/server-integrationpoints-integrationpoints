using System.Configuration;

namespace kCura.IntegrationPoints.Data.Tests
{
	public static class Config
	{
		public static string ConnectionString
		{
			get { return ConfigurationManager.AppSettings["connectionString"]; }
		}
	}
}