using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.LDAPSync.prototype.datasources.Implementations;

namespace kCura.LDAPSync.prototype.datasources
{
	public interface IDataConverterFactory
	{
		IDataSyncronizer GetConverter();
	}
	public class DataConverterFactory : IDataConverterFactory
	{
		public IDataSyncronizer GetConverter()
		{
			return new FileDataConverter("output.txt");
		}
	}
}
