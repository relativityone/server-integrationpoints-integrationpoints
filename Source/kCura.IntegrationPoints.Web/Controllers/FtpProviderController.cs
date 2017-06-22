using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Mvc;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.FtpProvider.Connection.Interfaces;
using kCura.IntegrationPoints.FtpProvider.Helpers;
using kCura.IntegrationPoints.FtpProvider.Helpers.Models;
using kCura.IntegrationPoints.Security;

namespace kCura.IntegrationPoints.Web.Controllers
{
    public class FtpProviderController : BaseController
    {
        internal IEncryptionManager _securityManager;
        private IConnectorFactory _connectorFactory;
        public FtpProviderController(IEncryptionManager securityManager, IConnectorFactory connectorFactory)
        {
            _securityManager = securityManager;
            _connectorFactory = connectorFactory;
        }

        public ActionResult GetDefaultFtpSettings()
        {
            var default_settings = new FtpProvider.Helpers.Models.Settings()
            {
                Port = 21,
                Protocol = FtpProvider.Helpers.ProtocolName.FTP,
                ColumnList = new List<FieldEntry>()
            };
            return View(default_settings);
        }

        [System.Web.Mvc.HttpPost]
        public HttpStatusCodeResult ValidateHostConnection(Settings settings)
        {
            var response = new HttpStatusCodeResult(HttpStatusCode.NotImplemented, "Nothing happened");

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
            settings.ValidatePort();
            try
            {
                using (var client = _connectorFactory.GetConnector(settings.Protocol, settings.Host, settings.Port, settings.Username, settings.Password))
                {
                    if (client.TestConnection())
                    {
                        response = new HttpStatusCodeResult(HttpStatusCode.OK);
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