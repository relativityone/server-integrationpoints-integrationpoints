using System.Collections.Generic;
using System.Data;

namespace kCura.IntegrationPoints.Core.Services.Conversion
{
	public interface IObjectBuilder
	{
		T BuildObject<T>(IDataRecord row, IEnumerable<string> columns); 
	}
}
