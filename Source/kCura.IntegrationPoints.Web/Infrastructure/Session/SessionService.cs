using Relativity.API;
using System;
using System.Runtime.CompilerServices;

namespace kCura.IntegrationPoints.Web.Infrastructure.Session
{
    public class SessionService : ISessionService
    {
        private readonly ICPHelper _connectionHelper;
        private readonly IAPILog _logger;

        public SessionService(ICPHelper connectionHelper, IAPILog logger)
        {
            _connectionHelper = connectionHelper;
            _logger = logger.ForContext<SessionService>();
        }

        public int? WorkspaceID => GetValueOrLogError(
            () => _connectionHelper.GetActiveCaseID()
        );

        public int? UserID => GetValueOrLogError(
            () => _connectionHelper.GetAuthenticationManager().UserInfo.ArtifactID
        );

        public int? WorkspaceUserID => GetValueOrLogError(
            () => _connectionHelper.GetAuthenticationManager().UserInfo.WorkspaceUserArtifactID
        );
        private int? GetValueOrLogError(Func<int> valueGetter, [CallerMemberName] string propertyName = "")
        {
            try
            {
                return valueGetter();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"{nameof(SessionService)} failed when executing {propertyName}");
            }

            return null;
        }
    }
}
