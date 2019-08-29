using System.Collections.Generic;

namespace Rip.SystemTests.RelativityServices.TestCases
{
	public class DocumentTestCase
	{
		public int ArtifactID { get; set; }
		public string ControlNumber { get; set; }
		public string FileName { get; set; }
		public IList<ImageTestCase> Images { get; set; }
	}
}
