using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using System.Web.Mvc;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.FtpProvider.Connection.Interfaces;
using kCura.IntegrationPoints.FtpProvider.Helpers;
using kCura.IntegrationPoints.FtpProvider.Helpers.Interfaces;
using kCura.IntegrationPoints.FtpProvider.Helpers.Models;
using kCura.IntegrationPoints.Web.Models;

namespace kCura.IntegrationPoints.Web.Controllers
{
    public class FtpProviderController : Controller
    {
        private readonly IConnectorFactory _connectorFactory;
	    private readonly ISettingsManager _settingsManager;

	    public FtpProviderController(IConnectorFactory connectorFactory, ISettingsManager settingsManager)
        {
            _connectorFactory = connectorFactory;
	        _settingsManager = settingsManager;
        }

        public ActionResult GetDefaultFtpSettings()
        {
            var defaultSettings = new SettingsViewModel()
            {
                Port = 21,
                Protocol = ProtocolName.FTP,
                ColumnList = new List<FieldEntry>()
            };
            return View(defaultSettings);
        }

        [System.Web.Mvc.HttpPost]
        public HttpStatusCodeResult ValidateHostConnection([FromBody] SynchronizerSettings synchronizerSettings)
        {
			var response = new HttpStatusCodeResult(HttpStatusCode.NotImplemented, "Nothing happened");

	        Settings settings = _settingsManager.DeserializeSettings(synchronizerSettings.Settings);
	        SecuredConfiguration securedConfiguration = _settingsManager.DeserializeCredentials(synchronizerSettings.Credentials);

			//immediately end if host value is non-standard
			if (settings.ValidateHost() == false)
            {
                response = new HttpStatusCodeResult(HttpStatusCode.BadRequest, ErrorMessage.INVALID_HOST_NAME);
                return response;
            }

            if (settings.ValidateCSVName() == false)
            {
                response = new HttpStatusCodeResult(HttpStatusCode.BadRequest, ErrorMessage.MISSING_CSV_FILE_NAME);
                return response;
            }
            settings.UpdatePort();
            try
            {
                using (var client = _connectorFactory.GetConnector(settings.Protocol, settings.Host, settings.Port, securedConfiguration.Username, securedConfiguration.Password))
                {
                    if (client.TestConnection())
                    {
                        response = new HttpStatusCodeResult(HttpStatusCode.NoContent);
                    }
                }
            }
            catch (Exception exception)
            {
                response = new HttpStatusCodeResult(HttpStatusCode.BadRequest, exception.Message);
            }
            return response;
        }
    }
}