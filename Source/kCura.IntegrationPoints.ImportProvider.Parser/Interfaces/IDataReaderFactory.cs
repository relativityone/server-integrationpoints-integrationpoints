using System.Data;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.ImportProvider.Parser.Interfaces
{
	public interface IDataReaderFactory
	{
		IDataReader GetDataReader(FieldMap[] fieldMaps, string options);
	}
}
