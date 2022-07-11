using Relativity.Sync.Configuration;

namespace Relativity.Sync.Storage
{
    internal class UserContextConfiguration : IUserContextConfiguration
    {
        public int ExecutingUserId { get; }

        public UserContextConfiguration(SyncJobParameters syncJobParameters)
        {
            ExecutingUserId = syncJobParameters.UserId;
        }
    }
}