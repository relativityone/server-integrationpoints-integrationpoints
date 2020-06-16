using System.Collections.Generic;
using System.Linq;

namespace Relativity.Sync.Tests.Performance.ARM.Contracts
{
	public class ArmConfiguration
	{
		public string RelativityWebApiUrl { get; set; }
		public IEnumerable<ArchiveLocation> ArmArchiveLocations { get; set; }
		public IEnumerable<object> EmailNotificationSettings { get; set; }

		public static ContractEnvelope<ArmConfiguration> GetRequest(string webAPIPath, string location)
		{
			return new ContractEnvelope<ArmConfiguration>
			{
				Contract = new ArmConfiguration
				{
					RelativityWebApiUrl = webAPIPath,
					ArmArchiveLocations = new List<ArchiveLocation>
					{
						new ArchiveLocation()
						{
							Location = location
						}
					},
					EmailNotificationSettings = Enumerable.Empty<object>()
				}
			};
		}
	}
}
