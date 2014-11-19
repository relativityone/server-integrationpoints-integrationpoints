using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Models.SyncConfiguration;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.Relativity.Client;
using Newtonsoft.Json;
using NSubstitute;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;

namespace kCura.IntegrationPoints.Synchronizers.RDO.Tests.Integration
{
	[TestFixture]
	public class RdoSynchronizerTest
	{
		[Test]
		[Explicit]
		public void FieldQueryTest()
		{
			//ARRANGE
			var client = new RSAPIClient(new Uri("http://localhost/Relativity.Services"), new IntegratedAuthCredentials())
			{
				APIOptions = {WorkspaceID = 1025517}
			};

			var rdo = new RdoSynchronizer(new RelativiityFieldQuery(client));
			//ASSERT

			rdo.GetFields(JsonConvert.SerializeObject(new RelativityConfiguration { ArtifactTypeID = 1000043 }));



		}

	}
}
