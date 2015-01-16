using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Core.Services.Conversion
{
	public interface IObjectBuilder
	{
		T BuildObject<T>(IDataRecord row, IEnumerable<string> columns); 
	}
}
