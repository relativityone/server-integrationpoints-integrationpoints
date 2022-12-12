using System.Net;
using System.Web.Http;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.LDAPProvider;
using kCura.IntegrationPoints.Web.Attributes;
using kCura.IntegrationPoints.Web.Models;
using Relativity.API;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
    public class LdapController : ApiController
    {
        private readonly ILDAPSettingsReader _settingsReader;
        private readonly ISerializer _serializer;
        private readonly ILDAPServiceFactory _ldapServiceFactory;
        private readonly IAPILog _apiLog;

        public LdapController(ICPHelper helper, ILDAPSettingsReader settingsReader, ISerializer serializer, ILDAPServiceFactory ldapServiceFactory)
        {
            _settingsReader = settingsReader;
            _serializer = serializer;
            _ldapServiceFactory = ldapServiceFactory;
            _apiLog = helper.GetLoggerFactory().GetLogger().ForContext<LdapController>();
        }

        [HttpPost]
        [LogApiExceptionFilter(Message = "Unable to retrieve LDAP provider settings.")]
        public IHttpActionResult GetViewFields([FromBody] object data)
        {
            LDAPSettings settings = _settingsReader.GetSettings(data.ToString());

            var ldapSummarySettingsModel = new LdapProviderSummaryPageSettingsModel(settings);
            string serializedModel = _serializer.Serialize(ldapSummarySettingsModel);
            return Ok(serializedModel);
        }

        [HttpPost]
        [LogApiExceptionFilter(Message = "Unable to validate LDAP connection.")]
        public IHttpActionResult CheckLdap([FromBody] SynchronizerSettings synchronizerSettings)
        {
            LDAPSettings settings = _settingsReader.GetSettings(synchronizerSettings.Settings);

            LDAPSecuredConfiguration securedConfiguration =
                _serializer.Deserialize<LDAPSecuredConfiguration>(synchronizerSettings.Credentials);

            var service = _ldapServiceFactory.Create(_apiLog, _serializer, settings, securedConfiguration);
            service.InitializeConnection();
            bool isAuthenticated = service.IsAuthenticated();
            if (isAuthenticated)
            {
                return StatusCode(HttpStatusCode.NoContent);
            }
            return Unauthorized();
        }
    }
}
