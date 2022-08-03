namespace kCura.IntegrationPoint.Tests.Core.Models
{
    public class UserModel
    {
        public int ArtifactID { get; }

        public string EmailAddress { get; }
        
        public string Password { get; }

        public UserModel(int artifactID, string emailAddress, string password)
        {
            ArtifactID = artifactID;
            EmailAddress = emailAddress;
            Password = password;
        }
    }
}