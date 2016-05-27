using System.Collections.Generic;
using kCura.IntegrationPoints.Contracts.Models;

namespace kCura.IntegrationPoints.Core.Managers
{
	public interface IErrorManager
	{
		void Create(IEnumerable<ErrorDTO> errors);
	}
}