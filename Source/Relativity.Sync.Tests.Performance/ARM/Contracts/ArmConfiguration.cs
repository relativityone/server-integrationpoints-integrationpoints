using System.Collections.Generic;
using System.Linq;

namespace Relativity.Sync.Tests.Performance.ARM.Contracts
{
	public class ArmConfiguration
	{
		public string RelativityWebApiUrl { get; set; }
		public IEnumerable<ArchiveLocation> ArmArchiveLocations { get; set; }
		public IEnumerable<object> EmailNotificationSettings { get; set; }

		public static ContractEnvelope<ArmConfiguration> GetRequest(string location)
		{
			return new ContractEnvelope<ArmConfiguration>
			{
				Contract = new ArmConfiguration
				{
					RelativityWebApiUrl = "https://emttest/RelativityWebAPI/",
					ArmArchiveLocations = new List<ArchiveLocation>
					{
						new ArchiveLocation()
						{
							ArchiveLocationType = 1,
							Location = location
						}
					},
					EmailNotificationSettings = Enumerable.Empty<object>()
				}
			};
		}
	}
}
