using System.Linq;
using System.Web;

namespace kCura.IntegrationPoints.Web.Context.UserContext
{
    internal class RequestHeadersUserContextService : IUserContext
    {
        private const string _USER_HEADER_VALUE = "X-IP-USERID";
        private const string _CASE_USER_HEADER_VALUE = "X-IP-CASEUSERID";
        private readonly HttpRequestBase _httpRequest;
        private readonly IUserContext _nextUserContextService;

        public RequestHeadersUserContextService(HttpRequestBase httpRequest, IUserContext nextUserContextService)
        {
            _httpRequest = httpRequest;
            _nextUserContextService = nextUserContextService;
        }

        public int GetUserID()
        {
            return GetRequestNumericValueByKey(_USER_HEADER_VALUE)
                   ?? _nextUserContextService.GetUserID();
        }

        public int GetWorkspaceUserID()
        {
            return GetRequestNumericValueByKey(_CASE_USER_HEADER_VALUE)
                   ?? _nextUserContextService.GetWorkspaceUserID();
        }

        private int? GetRequestNumericValueByKey(string key)
        {
            string firstValueForKey = GetFirstHeaderValueForKey(key);
            if (string.IsNullOrEmpty(firstValueForKey))
            {
                return null;
            }

            bool isValidInt = int.TryParse(firstValueForKey, out int returnValue);
            return isValidInt
                ? (int?)returnValue
                : null;
        }

        private string GetFirstHeaderValueForKey(string key)
        {
            return _httpRequest
                .Headers
                .GetValues(key)
                ?.FirstOrDefault();
        }
    }
}
