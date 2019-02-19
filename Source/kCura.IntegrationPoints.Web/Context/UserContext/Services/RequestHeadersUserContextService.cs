namespace kCura.IntegrationPoints.Web.Context.UserContext.Services
{
	internal class RequestHeadersUserContextService : IUserContextService
	{
		private const string _USER_HEADER_VALUE = "X-IP-USERID";
		private const string _CASE_USER_HEADER_VALUE = "X-IP-CASEUSERID";

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
			string[] sValues = System.Web.HttpContext.Current.Request.Headers.GetValues(key);
			if (sValues != null && sValues.Length > 0 && !string.IsNullOrEmpty(sValues[0]))
			{
				int.TryParse(sValues[0], out returnValue);
			}
			return returnValue;
		}
	}
}