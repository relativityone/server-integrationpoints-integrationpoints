using System.Collections.Generic;
using kCura.IntegrationPoints.Contracts.Models;

namespace kCura.IntegrationPoints.Contracts.Provider
{
	public interface IFieldProvider
	{
		IEnumerable<FieldEntry> GetFields(string options);
	}
}
