using Newtonsoft.Json;
using Relativity.Testing.Framework.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relativity.Sync.Tests.Performance.ARM
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>")]
	public class RequestProvider
	{
		private readonly ApiComponent _component;

		public RequestProvider(ApiComponent component)
		{
			_component = component;
		}

		public string ConfigurationRequest(string location)
		{
			var request = new
			{
				Contract = new
				{
					RelativityWebApiUrl = "https://emttest/RelativityWebAPI/",
					ArmArchiveLocations = new List<object>
					{
						new
						{
							ArchiveLocationType = 1,
							Location = location
						}
					},
					EmailNotificationSettings = Enumerable.Empty<object>()
				}
			};

			return JsonConvert.SerializeObject(request);
		}

		public string RestoreJobRequest(string archivedWorkspacePath)
		{
			var request = new
			{
				Contract = new
				{
					MatterId = RelativityConst.RELATIVITY_TEMPLATE_MATTER_ARTIFACT_ID,
					ArchivePath = archivedWorkspacePath,
					JobPriority = "Medium",
					ResourcePoolId = RelativityConst.DEFAULT_RESOURCE_POOL_ID,
					DatabaseServerId = RelativityConst.DATABASE_SERVER_ID,
					FileRepositoryId = RelativityConst.DEFAULT_FILE_REPOSITORY_ID,
					CacheLocationId = RelativityConst.DEFAULT_CACHE_LOCATION_ID,
					StructuredAnalyticsServerId = RelativityConst.STRUCTURED_ANALYTICS_SERVER_ID,
					ConceptualAnalyticsServerId = RelativityConst.CONCEPTUAL_ANALYTICS_SERVER_ID
				}
			};

			return JsonConvert.SerializeObject(request);
		}

		public string RunJobRequest(int jobID)
		{
			var request = new
			{
				JobId = jobID
			};
			
			return JsonConvert.SerializeObject(request);
		}
	}
}
