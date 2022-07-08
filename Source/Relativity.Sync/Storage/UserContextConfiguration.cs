using Relativity.Sync.Configuration;

namespace Relativity.Sync.Storage
{
    internal class UserContextConfiguration : IUserContextConfiguration
    {
        public int ExecutingUserId => _syncJobParameters.UserId;

        private readonly SyncJobParameters _syncJobParameters;

        public UserContextConfiguration(SyncJobParameters syncJobParameters)
        {
            _syncJobParameters = syncJobParameters;
        }
    }
}