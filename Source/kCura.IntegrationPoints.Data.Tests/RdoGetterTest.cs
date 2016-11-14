using System;
using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using kCura.Relativity.Client;
using NSubstitute;
using NUnit.Framework;
using ObjectType = kCura.Relativity.Client.DTOs.ObjectType;

namespace kCura.IntegrationPoints.Data.Tests
{
	[TestFixture]
	public class RdoGetterTest : TestBase
	{
		[SetUp]
		public override void SetUp()
		{
			
		}

		[Test]
		public void RelativityRdoQueryReturnsData()
		{
			var client = Substitute.For<IRSAPIClient>();
			var rdoMock = Substitute.For<RSAPIRdoQuery>(client);

			var testRdoQuery = new RdoGetter(rdoMock); 

			rdoMock.GetAllRdo().Returns(new List<ObjectType>
			{
				new ObjectType{ArtifactTypeID = 1},
				new ObjectType{ArtifactTypeID =  2}
			}); 
		
			Assert.AreEqual(testRdoQuery.GetAllRdo().Count, 2);
		}


		[Test]
		public void RelativityRdoQueryThrowsException()
		{
			var client = Substitute.For<IRSAPIClient>();
			var rdoMock = Substitute.For<RSAPIRdoQuery>(client);
			
			var testRdoQuery = new RdoGetter(rdoMock);

			rdoMock.GetAllRdo().Returns(x => { throw new Exception("No Relativity Dynamic Object found"); });
			Assert.Throws<Exception>(() => testRdoQuery.GetAllRdo());
			
		}
	}
}
