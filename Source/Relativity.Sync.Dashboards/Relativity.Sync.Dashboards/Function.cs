using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Refit;
using Relativity.Sync.Dashboards.Api;
using Relativity.Sync.Dashboards.Configuration;

namespace Relativity.Sync.Dashboards
{
    public static class Function
    {
	    const string AppSettingsFileName = "appsettings.json";

	    [FunctionName("Function")]
	    public static async Task<IActionResult> Run(
		    [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
		    HttpRequest req,
		    ILogger log)
	    {
		    try
		    {
			    log.LogInformation("Loading configuration from {fileName}", AppSettingsFileName);
			    string appSettingsJson = await File.ReadAllTextAsync(AppSettingsFileName).ConfigureAwait(false);
			    AppSettings appSettings = JsonConvert.DeserializeObject<AppSettings>(appSettingsJson);

			    List<SyncIssueDTO> syncIssues = await ReadSyncIssuesDTOAsync(log, req.Body).ConfigureAwait(false);

			    if (!syncIssues.Any())
			    {
				    return new NoContentResult();
			    }

			    IJiraApi jiraApi = CreateJiraApi(log, appSettings);
			    ISplunkApi splunkApi = CreateSplunkApi(log, appSettings);

			    SplunkKVStoreUpdater splunkKvStoreUpdater =
				    new SplunkKVStoreUpdater(appSettings, jiraApi, splunkApi, syncIssues, log);
			    await splunkKvStoreUpdater.UpdateSplunkKVStoreAsync().ConfigureAwait(false);
			    return new OkResult();
		    }
		    catch (JsonSerializationException ex)
		    {
			    return new BadRequestObjectResult(
				    new ObjectResult(new
				    {
					    Exception = ex
				    }));
		    }
		    catch (Exception ex)
		    {
			    var objectResult = new ObjectResult(new
			    {
				    Exception = ex
			    });
			    objectResult.StatusCode = 500;
			    return objectResult;
		    }
	    }

	    private static async Task<List<SyncIssueDTO>> ReadSyncIssuesDTOAsync(ILogger log, Stream stream)
        {
            log.LogInformation("Reading request stream");
            try
            {
	            using TextReader reader = new StreamReader(stream);
	            string body = await reader.ReadToEndAsync().ConfigureAwait(false);

	            var syncIssues = JsonConvert.DeserializeObject<List<SyncIssueDTO>>(body) ?? new List<SyncIssueDTO>();
				log.LogInformation("Found {count} items in the request.", syncIssues.Count);
	            return syncIssues;
            }
            catch (Exception ex)
            {
	            log.LogError(ex, "Error while reading stream from the request");
	            throw;
            }
        }

        private static IJiraApi CreateJiraApi(ILogger log, AppSettings settings)
        {
            log.LogInformation("Creating JIRA Api for URL: {jiraUrl}", settings.JiraURL);
	        try
	        {
				HttpClient jiraHttpClient = RestService.CreateHttpClient(settings.JiraURL, new RefitSettings());
				string authToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(settings.JiraUserName + ":" + settings.JiraUserPassword));
		        string authHeader = $"Basic {authToken}";
				jiraHttpClient.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(authHeader);

				IJiraApi jiraApi = RestService.For<IJiraApi>(jiraHttpClient);
		        return jiraApi;
	        }
	        catch (Exception ex)
	        {
		        log.LogError(ex, "Could not create JIRA Api.");
		        throw;
	        }
        }

        private static ISplunkApi CreateSplunkApi(ILogger log, AppSettings settings)
        {
			log.LogInformation("Creating Splunk API for URL: {splunkURL}", settings.SplunkURL);
	        try
	        {
		        HttpClient splunkHttpClient = RestService.CreateHttpClient(settings.SplunkURL, new RefitSettings()
		        {
			        HttpMessageHandlerFactory = () => new HttpClientHandler()
			        {
				        ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true
			        }
		        });
		        splunkHttpClient.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse($"Bearer {settings.SplunkAccessToken}");
            
		        ISplunkApi splunkApi = RestService.For<ISplunkApi>(splunkHttpClient);
		        return splunkApi;
	        }
	        catch (Exception ex)
	        {
		        log.LogError(ex, "Could not create Splunk API.");
		        throw;
	        }
        }
    }
}
