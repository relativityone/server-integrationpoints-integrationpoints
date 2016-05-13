using System;
using System.Net;
using System.Net.Http;
using System.Text;

namespace kCura.IntegrationPoint.Tests.Core
{
	public class Rest : HelperBase
	{
		private const string _JSON_MIME = "application/json";

		public Rest(Helper helper) : base(helper)
		{
		}

		public string PostRequestAsJson(string serviceMethod, bool isHttps, string parameter = null)
		{
			Uri baseAddress = new Uri(SharedVariables.RestServer);
			WebRequestHandler handler = new WebRequestHandler();

			if (isHttps)
			{
				handler.ServerCertificateValidationCallback = ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => true;
			}

			using (HttpClient httpClient = new HttpClient(handler))
			{
				httpClient.BaseAddress = baseAddress;

				//Set header information
				string authorizationBase64 = GetBase64String(string.Format("{0}:{1}", SharedVariables.RelativityUserName, SharedVariables.RelativityPassword));
				string authorizationHeader = string.Format("Basic {0}", authorizationBase64);
				httpClient.DefaultRequestHeaders.Add("Authorization", authorizationHeader);
				httpClient.DefaultRequestHeaders.Add("X-CSRF-Header", String.Empty);

				//Assign parameter if needed
				HttpContent content = null;
				if (parameter != null)
				{
					content = new StringContent(parameter, Encoding.UTF8, _JSON_MIME);
				}

				string output = null;
				try
				{
					HttpResponseMessage response = httpClient.PostAsync(serviceMethod, content).Result;
					if (!response.IsSuccessStatusCode)
					{
						string errorMessage = string.Format("Failed submitting post request. Response Error: {0}.", response.Content.ReadAsStringAsync());
						throw new Exception(errorMessage);
					}
					output = response.Content.ReadAsStringAsync().Result;
				}
				catch (Exception ex)
				{
					string errorMessage = string.Format("An error occurred when attempting to submit post request. {0}.", ex.Message);
					throw new Exception(errorMessage);
				}
				return output;
			}
		}

		public string DeleteRequestAsJson(string restServer, string serviceMethod, string username, string password, bool isHttps)
		{
			Uri baseAddress = new Uri(SharedVariables.RestServer);
			WebRequestHandler handler = new WebRequestHandler();

			if (isHttps)
			{
				handler.ServerCertificateValidationCallback = ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => true;
			}

			using (HttpClient httpClient = new HttpClient(handler))
			{
				httpClient.BaseAddress = baseAddress;

				//Set header information
				string authorizationBase64 = GetBase64String(string.Format("{0}:{1}", username, password));
				string authorizationHeader = string.Format("Basic {0}", authorizationBase64);
				httpClient.DefaultRequestHeaders.Add("Authorization", authorizationHeader);
				httpClient.DefaultRequestHeaders.Add("X-CSRF-Header", String.Empty);

				string output = null;
				try
				{
					HttpResponseMessage response = httpClient.DeleteAsync(serviceMethod).Result;
					if (!response.IsSuccessStatusCode)
					{
						string errorMessage = string.Format("Failed submitting delete request. Response Error: {0}.", response.Content.ReadAsStringAsync());
						throw new Exception(errorMessage);
					}
					output = response.Content.ReadAsStringAsync().Result;
				}
				catch (Exception ex)
				{
					string errorMessage = string.Format("An error occurred when attempting to submit delete request. {0}.", ex.Message);
					throw new Exception(errorMessage);
				}
				return output;
			}
		}

		private string GetBase64String(string stringToConvertToBase64)
		{
			string base64String = Convert.ToBase64String(Encoding.ASCII.GetBytes(stringToConvertToBase64));
			return base64String;
		}
	}
}