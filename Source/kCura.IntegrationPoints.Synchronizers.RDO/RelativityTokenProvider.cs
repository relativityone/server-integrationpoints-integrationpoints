using kCura.IntegrationPoints.Domain.Authentication;
using Relativity.DataExchange;

namespace kCura.IntegrationPoints.Synchronizers.RDO
{
    public class RelativityTokenProvider : IRelativityTokenProvider
    {
        private readonly IAuthTokenGenerator _authTokenGenerator;

        public RelativityTokenProvider(IAuthTokenGenerator authTokenGenerator)
        {
            _authTokenGenerator = authTokenGenerator;
        }

        public string GetToken()
        {
            return _authTokenGenerator.GetAuthToken();
        }
    }
}
