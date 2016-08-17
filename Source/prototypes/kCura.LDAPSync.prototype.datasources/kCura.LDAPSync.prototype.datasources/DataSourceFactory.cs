using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.LDAPSync.prototype.datasources.Implementations;

namespace kCura.LDAPSync.prototype.datasources
{
	public interface IDataSourceFactory
	{
		IDataSourceProvider GetDataSource();
	}

	public class DataSourceFactory : IDataSourceFactory
	{
		public IDataSourceProvider GetDataSource()
		{
			return new JsonDataSource("source.json");
		}
	}
}
