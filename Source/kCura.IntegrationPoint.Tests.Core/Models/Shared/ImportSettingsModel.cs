using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoint.Tests.Core.Models.Shared
{
	public class ImportSettingsModel
	{
		public List<Tuple<string, string>> FieldMapping { get; set; }

		public OverwriteType Overwrite { get; set; }
	}
}
