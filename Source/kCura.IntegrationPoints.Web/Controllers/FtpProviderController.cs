using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using System.Web.Mvc;
using kCura.IntegrationPoints.FtpProvider.Connection.Interfaces;
using kCura.IntegrationPoints.FtpProvider.Helpers;
using kCura.IntegrationPoints.FtpProvider.Helpers.Interfaces;
using kCura.IntegrationPoints.FtpProvider.Helpers.Models;
using kCura.IntegrationPoints.Web.Models;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.API;

namespace kCura.IntegrationPoints.Web.Controllers
{
    public class FtpProviderController : Controller
    {
        private readonly IConnectorFactory _connectorFactory;
        private readonly ISettingsManager _settingsManager;
        private readonly ICPHelper _helper;
        private readonly IAPILog _logger;

        public FtpProviderController(IConnectorFactory connectorFactory, ISettingsManager settingsManager, ICPHelper helper)
        {
            _connectorFactory = connectorFactory;
            _settingsManager = settingsManager;
            _helper = helper;
            _logger = _helper.GetLoggerFactory().GetLogger()
                .ForContext<FtpProviderController>()
                .ForContext("CorrelationId", Guid.NewGuid(), false);
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
            _logger.LogInformation("Host validation has been started...");

            var response = new HttpStatusCodeResult(HttpStatusCode.NotImplemented, "Nothing happened");

            Settings settings = _settingsManager.DeserializeSettings(synchronizerSettings.Settings);
            _logger.LogInformation("FTP Settings: {settings}", synchronizerSettings.Settings);
            SecuredConfiguration securedConfiguration = _settingsManager.DeserializeCredentials(synchronizerSettings.Credentials);

            //immediately end if host value is non-standard
            if (settings.ValidateHost() == false)
            {
                response = new HttpStatusCodeResult(HttpStatusCode.BadRequest, ErrorMessage.INVALID_HOST_NAME);
                return response;
            }
            _logger.LogInformation("Host has been validated");

            if (settings.ValidateCSVName() == false)
            {
                response = new HttpStatusCodeResult(HttpStatusCode.BadRequest, ErrorMessage.MISSING_CSV_FILE_NAME);
                return response;
            }
            _logger.LogInformation("CSV file  has been validated");

            settings.UpdatePort();

            try
            {
                _logger.LogInformation("Trying to establish {protocol} connection", settings.Protocol);
                using (var client = _connectorFactory.GetConnector(settings.Protocol, settings.Host, settings.Port, securedConfiguration.Username, securedConfiguration.Password))
                {
                    _logger.LogInformation("Test {protocol} connection", settings.Protocol);
                    if (client.TestConnection())
                    {
                        _logger.LogInformation("Connected.");
                        response = new HttpStatusCodeResult(HttpStatusCode.NoContent);
                    }
                    else
                    {
                        const string message = "Cannot connect to specified host.";
                        _logger.LogInformation(message);
                        response = new HttpStatusCodeResult(HttpStatusCode.Forbidden, message);
                    }
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error occured during establishing the connection");
                response = new HttpStatusCodeResult(HttpStatusCode.BadRequest, exception.Message);
            }
            return response;
        }
    }
}