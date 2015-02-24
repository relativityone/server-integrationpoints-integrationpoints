using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
