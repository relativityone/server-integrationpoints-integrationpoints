using System;
using System.Net;
using System.Web.Http;
using System.Web.Mvc;
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

        [System.Web.Mvc.HttpPost]
        public JsonResult Encrypt(string value)
        {
            string message = value.ToString();
            JsonResult retVal = new JsonResult();
            try
            {
                if (message != null)
                {
                    retVal.Data = _securityManager.Encrypt(message);
                }
            }
            catch (Exception ex)
            {
                Response.StatusCode = (Int32)HttpStatusCode.BadRequest;
                retVal.Data = ex.ToString();
            }
            return retVal;
        }

        [System.Web.Mvc.HttpPost]
        public JsonResult Decrypt(string value)
        {
            string message = value.ToString();
            var retVal = new JsonResult();
            try
            {
                //check if it's already decrypted
                Newtonsoft.Json.JsonConvert.DeserializeObject<Settings>(message);
                retVal.Data = message;
            }
            catch
            {
                try
                {
                    message = Newtonsoft.Json.JsonConvert.DeserializeObject<string>(message);
                }
                catch
                {
                    /*just a regular string so we pass and decrypt it.*/
                }

                try
                {
                    retVal.Data = _securityManager.Decrypt(message);
                }
                catch (Exception ex)
                {
                    Response.StatusCode = (Int32)HttpStatusCode.BadRequest;
                    retVal.Data = ex.ToString();
                }
            }
            return retVal;
        }

        public ActionResult GetDefaultFtpSettings()
        {
            var default_settings = new FtpProvider.Helpers.Models.Settings()
            {
                Port = 21,
                Protocol = FtpProvider.Helpers.ProtocolName.FTP
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

            try
            {
                using (var client = GetClient(settings))
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

        public virtual IFtpConnector GetClient(Settings settings)
        {
            IFtpConnector client = null;

            if (settings.Protocol == ProtocolName.FTP)
            {
                client = _connectorFactory.CreateFtpConnector(settings.Host, settings.Port.GetValueOrDefault(21), settings.Username, settings.Password);
            }
            else
            {
                client = _connectorFactory.CreateSftpConnector(settings.Host, settings.Port.GetValueOrDefault(22), settings.Username, settings.Password);
            }

            return client;
        }

    }
}