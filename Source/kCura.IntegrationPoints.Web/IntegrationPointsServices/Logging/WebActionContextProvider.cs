using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using kCura.IntegrationPoints.Common.Extensions.DotNet;

namespace kCura.IntegrationPoints.Web.IntegrationPointsServices.Logging
{
    internal class WebActionContextProvider : IWebCorrelationContextProvider
    {
        /// <summary>
        /// Expression for mapping IntegrationPointsAPIController.Edit method (New record). Example:
        /// https://test.relativity.corp/Relativity/CustomPages/DCF6E9D1-22B6-4DA3-98F6-41381E93C30C/1018658/api/IntegrationPointsAPI/1040486?_=1510730418617
        /// </summary>
        private const string _JOB_EDIT_ACTION_REG_EX =
            @"^https:\/\/.*\/(Relativity)\/.*\/DCF6E9D1-22B6-4DA3-98F6-41381E93C30C\/(\d)*\/api\/\bIntegrationPointsAPI\b\/([0-9]{2,})\?_=(\d+)$";

        /// <summary>
        /// Expression for mapping IntegrationPointsAPIController.Edit method (New existing record). Example
        /// https://test.relativity.corp/Relativity/CustomPages/DCF6E9D1-22B6-4DA3-98F6-41381E93C30C/1018658/api/IntegrationPointsAPI/0?_=1510730418617
        /// </summary>
        private const string _JOB_NEW_ACTION_REG_EX =
            @"^https:\/\/.*\/(Relativity)\/.*\/DCF6E9D1-22B6-4DA3-98F6-41381E93C30C\/(\d)*\/api\/\bIntegrationPointsAPI\b\/0\?_=(\d+)$";


        /// <summary>
        /// Expression for mapping JobController.Run method. Example:
        /// https://test.relativity.corp/Relativity/CustomPages/DCF6E9D1-22B6-4DA3-98F6-41381E93C30C/1018658/api/Job
        /// </summary>
        private const string _JOB_RUN_ACTION_REG_EX = @"^https:\/\/.*\/(Relativity)\/.*\/DCF6E9D1-22B6-4DA3-98F6-41381E93C30C\/(\d)*\/api\/\bJob\b";

        /// <summary>
        /// Expression for mapping IntegrationPointsAPIController.Edit method (New record). Example:
        /// https://test.relativity.corp/Relativity/CustomPages/DCF6E9D1-22B6-4DA3-98F6-41381E93C30C/1018658/api/IntegrationPointsAPI/1040486?_=1510730418617
        /// </summary>
        private const string _JOB_SAVE_AS_PROFILE_ACTION_REG_EX = @"^https:\/\/.*\/(Relativity)\/.*\/DCF6E9D1-22B6-4DA3-98F6-41381E93C30C\/(\d)*\/api\/IntegrationPointProfilesAPI\b\/SaveAsProfile\/(.*)";

        /// <summary>
        /// Expression for mapping IntegrationPointsAPIController.Edit method (New record). Example:
        /// https://test.relativity.corp/Relativity/CustomPages/DCF6E9D1-22B6-4DA3-98F6-41381E93C30C/1018658/api/IntegrationPointsAPI/1040486?_=1510730418617
        /// </summary>
        private const string _JOB_EDIT_PROFILE_ACTION_REG_EX =
            @"^https:\/\/.*\/(Relativity)\/.*\/DCF6E9D1-22B6-4DA3-98F6-41381E93C30C\/(\d)*\/api\/\bIntegrationPointProfilesAPI\b\/([0-9]{2,})\?_=(\d+)$";

        /// <summary>
        /// Expression for mapping IntegrationPointsAPIController.Edit method (New existing record). Example
        /// https://test.relativity.corp/Relativity/CustomPages/DCF6E9D1-22B6-4DA3-98F6-41381E93C30C/1018658/api/IntegrationPointsAPI/0?_=1510730418617
        /// </summary>
        private const string _JOB_NEW_PROFILE_ACTION_REG_EX =
            @"^https:\/\/.*\/(Relativity)\/.*\/DCF6E9D1-22B6-4DA3-98F6-41381E93C30C\/(\d)*\/api\/\bIntegrationPointProfilesAPI\b\/0\?_=(\d+)$";

        private readonly ICacheHolder _cacheHolder;

        private static readonly Dictionary<Regex, string> _matchedRegExActions;

        private static readonly Regex _jobEditRegEx = new Regex(_JOB_EDIT_ACTION_REG_EX);
        private static readonly Regex _jobNewRegEx = new Regex(_JOB_NEW_ACTION_REG_EX);
        private static readonly Regex _jobRunRegEx = new Regex(_JOB_RUN_ACTION_REG_EX);
        private static readonly Regex _jobSaveAsProfileRegEx = new Regex(_JOB_SAVE_AS_PROFILE_ACTION_REG_EX);
        private static readonly Regex _jobEditProfileRegEx = new Regex(_JOB_EDIT_PROFILE_ACTION_REG_EX);
        private static readonly Regex _jobNewProfileRegEx = new Regex(_JOB_NEW_PROFILE_ACTION_REG_EX);

        public const string JOB_EDIT_ACTION = "EditJob";
        public const string JOB_NEW_ACTION = "NewJob";
        public const string JOB_RUN_ACTION = "RunJob";
        public const string JOB_SAVE_AS_PROFILE_ACTION = "SaveAsProfileJob";
        public const string JOB_EDIT_PROFILE_ACTION = "EditProfile";
        public const string JOB_NEW_PROFILE_ACTION = "NewProfile";

        static WebActionContextProvider()
        {
            _matchedRegExActions = new Dictionary<Regex, string>()
            {
                { _jobNewRegEx, JOB_NEW_ACTION },
                { _jobEditRegEx, JOB_EDIT_ACTION },
                { _jobRunRegEx, JOB_RUN_ACTION },
                { _jobSaveAsProfileRegEx, JOB_SAVE_AS_PROFILE_ACTION },
                { _jobNewProfileRegEx, JOB_NEW_PROFILE_ACTION},
                { _jobEditProfileRegEx, JOB_EDIT_PROFILE_ACTION}
            };
        }

        public WebActionContextProvider(ICacheHolder cacheHolder)
        {
            _cacheHolder = cacheHolder;
        }

        public WebActionContext GetDetails(string url, int userId)
        {
            string parsedAction = ParseAction(url);
            string key = userId.ToString();
            if (parsedAction.IsNullOrEmpty())
            {
                WebActionContext cachedAction = _cacheHolder.GetObject<WebActionContext>(key);
                if (cachedAction == null)
                {
                    cachedAction = new WebActionContext(JOB_EDIT_ACTION, Guid.NewGuid());
                }
                return cachedAction;
            }

            var newActionContext = new WebActionContext(parsedAction, Guid.NewGuid());
            _cacheHolder.SetObject(key, newActionContext);
            return newActionContext;
        }

        private string ParseAction(string url)
        {
            KeyValuePair<Regex, string> foundEntry = _matchedRegExActions.FirstOrDefault(item => item.Key.IsMatch(url));

            return foundEntry.Equals(default(KeyValuePair<Regex, string>)) ? string.Empty : foundEntry.Value;
        }
    }
}
