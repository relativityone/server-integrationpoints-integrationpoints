using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Contracts.Models;

namespace kCura.IntegrationPoints.Core.Managers.Implementations
{
	public class FieldManager : IFieldManager
	{
		public bool FieldExists(Guid fieldGuid)
		{
			return false;
		}

		public int Create(ArtifactFieldDTO field)
		{
			return -1;
		}
	}
}
