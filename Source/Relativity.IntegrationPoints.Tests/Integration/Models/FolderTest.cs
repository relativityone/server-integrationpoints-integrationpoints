using Relativity.Services.Objects.DataContracts;

namespace Relativity.IntegrationPoints.Tests.Integration.Models
{
	public class FolderTest : RdoTestBase
	{
		public string Name { get; set; }

		public FolderTest() : base("Folder")
		{
		}

		public override RelativityObject ToRelativityObject()
		{
			throw new System.NotImplementedException();
		}
	}
}
