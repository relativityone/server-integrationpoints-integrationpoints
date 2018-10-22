namespace kCura.IntegrationPoints.UITests.Configuration
{
	public class RelativityUser
	{
		public RelativityUser(int artifactId, string email, string password)
		{
			ArtifactId = artifactId;
			Email = email;
			Password = password;
			CreatedInTest = true;
		}

		public RelativityUser(string email, string password)
		{
			Email = email;
			Password = password;
			CreatedInTest = false;
		}

		public int ArtifactId { get; }

		public bool CreatedInTest { get; }

		public string Email { get; }

		public string Password { get; }
	}
}
