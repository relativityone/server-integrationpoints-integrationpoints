using System.Collections.Generic;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace kCura.IntegrationPoints.Core.Tests.Unit
{
	[TestClass]
	public class RdoFilterTest
	{
		[TestMethod]
		public void rdoFilterRemovesAllHistoryAndEvents()
		{
			//ARRANGEk
			var client = NSubstitute.Substitute.For<IRSAPIClient>();
			var rdoMock = NSubstitute.Substitute.For<RelativityRdoQuery>(client);
			var rdoFilter = new RdoFilter(rdoMock);
			rdoMock.GetAllRdo().Returns( new List<ObjectType>
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

			var actual = rdoFilter.FilterRdo();

			Assert.AreEqual(expected.Count, actual.Count);
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
