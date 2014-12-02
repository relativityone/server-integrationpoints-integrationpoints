using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data.Tests.Integration;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Data.Tests.Unit
{
	public class RdoGetterTest
	{
		[Test]
		public void RelativityRdoQueryReturnsData()
		{
			var client = NSubstitute.Substitute.For<IRSAPIClient>();
			var rdoMock = NSubstitute.Substitute.For<RelativityRdoQuery>(client);

			var testRdoQuery = new RdoGetter(rdoMock); 

			rdoMock.GetAllRdo().Returns(new List<ObjectType>
			{
				new ObjectType{ArtifactTypeID = 1},
				new ObjectType{ArtifactTypeID =  2}
			}); 
		
			Assert.AreEqual(testRdoQuery.getAllRdo().Count, 2);
		}


		[Test]
		public void RelativityRdoQueryThrowsException()
		{
			var client = NSubstitute.Substitute.For<IRSAPIClient>();
			var rdoMock = NSubstitute.Substitute.For<RelativityRdoQuery>(client);
			
			var testRdoQuery = new RdoGetter(rdoMock);

			rdoMock.GetAllRdo().Returns(x => { throw new Exception("No Relativity Dynamic Object found"); });
			Assert.Throws<Exception>(() => testRdoQuery.getAllRdo());
			
		}
	}
}
