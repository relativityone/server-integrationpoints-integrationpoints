using Relativity.API;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks
{
    class FakeUserInfo : IUserInfo
    {
        public FakeUserInfo(int userArtifactId)
        {
            WorkspaceUserArtifactID = userArtifactId;
            ArtifactID = userArtifactId;
        }

        public int WorkspaceUserArtifactID { get; }
        public int ArtifactID { get; }
        public string FirstName { get; }
        public string LastName { get; }
        public string FullName { get; }
        public string EmailAddress { get; }
        public int AuditWorkspaceUserArtifactID { get; }
        public int AuditArtifactID { get; }
    }
}
