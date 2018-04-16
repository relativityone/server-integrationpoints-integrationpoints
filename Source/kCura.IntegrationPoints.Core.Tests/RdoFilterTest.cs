using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Models;
using NSubstitute;
using NUnit.Framework;

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
			//ARRANGE
			var rdoMock = Substitute.For<IObjectTypeQuery>();
			var context = Substitute.For<ICaseServiceContext>();
			context.WorkspaceUserID = 3;
			var rdoFilter = new RdoFilter(rdoMock, context);
			rdoMock.GetAllTypes(Arg.Any<int>()).Returns(new List<ObjectTypeDTO>
			{
				new ObjectTypeDTO
				{
					Name = "History"
				},
				new ObjectTypeDTO
				{
					Name = "Document"
				},
				new ObjectTypeDTO
				{
					Name = "Event Handler"
				},
				new ObjectTypeDTO
				{
				  Name =  "Event Viewer"
				},

			});

			var expected = new List<ObjectTypeDTO>
			{
				new ObjectTypeDTO
				{
					Name = "Document"
				},
				new ObjectTypeDTO
				{
					Name = "Event Viewer"
				}
			};

			// act 
			var actual = rdoFilter.GetAllViewableRdos().ToList();

			// assert 
			Assert.AreEqual(expected.Count, actual.Count);
			for (int i = 0; i < expected.Count; i++)
			{
				if (expected[i].Name != actual[i].Name)
				{
					Assert.Fail("Values not same");
				}
			}
		}
	}
}
