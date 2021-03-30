namespace Relativity.IntegrationPoints.Tests.Integration.Mocks
{
	public class FakeUser
	{
		public int ArtifactId { get; set; }

		public bool IsAdmin { get; set; }

		public FakeUser()
		{
			ArtifactId = ArtifactProvider.NextId();
		}
	}
}
