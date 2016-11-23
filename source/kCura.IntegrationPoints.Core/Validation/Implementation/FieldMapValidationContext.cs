using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Synchronizers.RDO;

namespace kCura.IntegrationPoints.Core.Validation.Implementation
{
	public class FieldMapValidationContext
	{
		public SourceConfiguration SourceConfiguration { get; set; }
		public ImportSettings DestinationConfiguration { get; set; }
	}
}
