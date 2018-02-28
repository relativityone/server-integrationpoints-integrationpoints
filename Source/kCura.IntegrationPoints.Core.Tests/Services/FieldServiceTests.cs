using System;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.Relativity.Client;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Services
{
    public class FieldServiceTests : TestBase
    {
		private IRSAPIClient _client;
		private IChoiceService _choiceService;

	    private FieldService sut;

        public override void SetUp()
        {
			var workspaceId = 123;

			_client = Substitute.For<IRSAPIClient>();
			_client.APIOptions = new APIOptions(workspaceId);

			_choiceService = Substitute.For<IChoiceService>();

			sut = new FieldService(_choiceService, _client);
        }

		[Test]
		public void GetTextFields_Exception()
		{
			//ARRANGE
			var message = "This is an example failure";
			var result = new QueryResult
			{
				Success = false,
				Message = message
			};

			_client.Query(Arg.Any<APIOptions>(), Arg.Any<Query>())
				.Returns(result);
			
			// ACT/ASSERT
			Assert.That(() => sut.GetTextFields(0, false),
				Throws.Exception
					.TypeOf<IntegrationPointsException>()
					.With.Property("Message")
					.EqualTo(message));
		}


    }
}
