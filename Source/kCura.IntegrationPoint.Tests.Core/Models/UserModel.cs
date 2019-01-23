namespace kCura.IntegrationPoint.Tests.Core.Models
{
	public class UserModel
	{
		public int ArtifactId { get; }

		public string EmailAddress { get; }
		
		public string Password { get; }

		public UserModel(int artifactId, string emailAddress, string password)
		{
			ArtifactId = artifactId;
			EmailAddress = emailAddress;
			Password = password;
		}
	}
}