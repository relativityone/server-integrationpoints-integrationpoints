using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Core.Models;

namespace kCura.IntegrationPoints.Core.Services.Provider
{
	public interface IFieldProvider
	{
		IEnumerable<FieldEntry> GetFields(string options);
	}
}
