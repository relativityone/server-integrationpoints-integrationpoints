using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.Relativity.Client;
using NSubstitute;
using NUnit.Framework;
using ObjectType = kCura.Relativity.Client.DTOs.ObjectType;

namespace kCura.IntegrationPoints.Core.Tests
{
	[TestFixture]
	public class RdoFilterTest : TestBase
	{
		[SetUp]
		public override void SetUp()
		{
			
		}

		[Test]
		public void rdoFilterRemovesAllHistoryAndEvents()
		{
			//ARRANGEk
			var client = NSubstitute.Substitute.For<IRSAPIClient>();
			var rdoMock = NSubstitute.Substitute.For<RSAPIRdoQuery>(client);
			var context = NSubstitute.Substitute.For<ICaseServiceContext>();
			context.WorkspaceUserID = 3;
			var rdoFilter = new RdoFilter(rdoMock, context);
			rdoMock.GetAllRdo().Returns(new List<ObjectType>
			{
				 new ObjectType
				{
					Name = "History"
				},
				 new ObjectType
				{
					Name = "Document"
				},
				 new ObjectType
				{
					Name = "Event Handler"
				},
			new ObjectType
				{
				  Name =  "Event Viewer"
				},
				
			});
			// act 
			var expected = new List<ObjectType>
			{
				new ObjectType
				{
					Name = "Document"
				},
				new ObjectType
				{
					Name = "Event Viewer"
				}
			};

			var actual = rdoFilter.GetAllViewableRdos().ToList();

			Assert.AreEqual(expected.Count, actual.Count());
			for (int i = 0; i < expected.Count; i++)
			{
				if (expected[i].Name != actual[i].Name)
				{
					Assert.Fail("Values not same");
				}
			}

			// assert 

		}
	}
}
