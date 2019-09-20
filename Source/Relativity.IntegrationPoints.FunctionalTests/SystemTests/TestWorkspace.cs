namespace Relativity.IntegrationPoints.FunctionalTests.SystemTests
{
	public class TestWorkspace
	{
		public int ArtifactId { get; }
		public string Name { get; }

		public TestWorkspace(int artifactId, string name)
		{
			ArtifactId = artifactId;
			Name = name;
		}
	}
}
