using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Core.Services
{
	public class SourceType
	{
		public string Name { get; set; }
		public string ID { get; set; }
	}

	public class SourceTypeFactory
	{
		public virtual IEnumerable<SourceType> GetSourceTypes()
		{
			//hardcoded for now but you could use reflection to get the types
			yield return new SourceType
				{
					Name = "LDAP",
					ID = "ldap"
				};
		}
	}
}
