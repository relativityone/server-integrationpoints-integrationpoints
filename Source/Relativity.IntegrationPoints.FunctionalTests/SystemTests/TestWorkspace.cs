namespace Relativity.IntegrationPoints.FunctionalTests.SystemTests
{
	public class TestWorkspace
	{
		public int ArtifactID { get; }
		public string Name { get; }

		public TestWorkspace(int artifactID, string name)
		{
			ArtifactID = artifactID;
			Name = name;
		}
	}
}
