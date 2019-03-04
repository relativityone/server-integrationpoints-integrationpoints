using System.Linq;
using System.Web;

namespace kCura.IntegrationPoints.Web.Context.UserContext.Services
{
	internal class RequestHeadersUserContextService : IUserContextService
	{
		private const string _USER_HEADER_VALUE = "X-IP-USERID";
		private const string _CASE_USER_HEADER_VALUE = "X-IP-CASEUSERID";

		private readonly HttpRequestBase _httpRequest;

		public RequestHeadersUserContextService(HttpRequestBase httpRequest)
		{
			_httpRequest = httpRequest;
		}

		public int GetUserID()
		{
			return GetRequestNumericValueByKey(_USER_HEADER_VALUE);
		}

		public int GetWorkspaceUserID()
		{
			return GetRequestNumericValueByKey(_CASE_USER_HEADER_VALUE);
		}

		private int GetRequestNumericValueByKey(string key)
		{
			int returnValue = 0;
			string firstValueForKey = GetFirstHeaderValueForKey(key);
			if (!string.IsNullOrEmpty(firstValueForKey))
			{
				int.TryParse(firstValueForKey, out returnValue);
			}
			return returnValue;
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