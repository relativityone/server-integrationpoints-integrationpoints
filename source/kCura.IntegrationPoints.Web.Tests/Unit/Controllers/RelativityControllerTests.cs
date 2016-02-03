using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Results;
using kCura.IntegrationPoints.Web.Controllers.API;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Web.Tests.Unit.Controllers
{
	[TestFixture]
	public class RelativityControllerTests
	{
		private RelativityController _instance;
		private const int WORKSPACE_ARTIFACT_ID = 10293;
		private const int SAVED_SEARCH_ID = 123434;

		[SetUp]
		public void SetUp()
		{
			_instance = new RelativityController();	
		}

		[Test]
		public void GetViewFields_CorrectParams_ReturnsCorrectResult()
		{
			// Arrange
			string data = Newtonsoft.Json.JsonConvert.SerializeObject(new
			{
				WorkspaceArtifactId = WORKSPACE_ARTIFACT_ID,
				SavedSearchArtifactId = SAVED_SEARCH_ID
			});

			var expectedResult = new List<KeyValuePair<string, int>>();
			expectedResult.Add(new KeyValuePair<string, int>(kCura.IntegrationPoints.DocumentTransferProvider.Shared.Constants.TARGET_WORKSPACE_ID, WORKSPACE_ARTIFACT_ID));
			expectedResult.Add(new KeyValuePair<string, int>(kCura.IntegrationPoints.DocumentTransferProvider.Shared.Constants.SAVED_SEARCH_ID, SAVED_SEARCH_ID));

			// Act
			IHttpActionResult result = _instance.GetViewFields(data);

			// Assert
			Assert.IsNotNull(result, "Either no result was returned or the result was not of type OkResult");
			Assert.IsInstanceOf<OkNegotiatedContentResult<List<KeyValuePair<string, int>>>>(result);
			Assert.IsTrue(this.ResultsMatch(expectedResult,
				((OkNegotiatedContentResult<List<KeyValuePair<string, int>>>)result).Content));
		}

		[Test]
		public void GetViewFields_MissingParam_ReturnsBadRequest()
		{
			// Arrange
			string data = Newtonsoft.Json.JsonConvert.SerializeObject(new
			{
				WorkspaceArtifactId = WORKSPACE_ARTIFACT_ID,
			});

			// Act
			IHttpActionResult result = _instance.GetViewFields(data);

			// Assert
			Assert.IsNotNull(result);
			Assert.IsInstanceOf<BadRequestErrorMessageResult>(result);
			Assert.AreEqual(kCura.IntegrationPoints.DocumentTransferProvider.Shared.Constants.INVALID_PARAMETERS, ((BadRequestErrorMessageResult)result).Message);
		}

		[Test]
		public void GetViewFields_BadParams_ReturnsBadRequest()
		{
			// Arrange
			string data = Newtonsoft.Json.JsonConvert.SerializeObject(new
			{
				WorkspaceArtifactId = WORKSPACE_ARTIFACT_ID,
				SAVED_SEARCH_ID = "WooplaH!"
			});

			// Act
			IHttpActionResult result = _instance.GetViewFields(data);

			// Assert
			Assert.IsNotNull(result, "Either no result was returned or the result was not of type OkResult");
			Assert.IsInstanceOf<BadRequestErrorMessageResult>(result);
			Assert.AreEqual(kCura.IntegrationPoints.DocumentTransferProvider.Shared.Constants.INVALID_PARAMETERS, ((BadRequestErrorMessageResult)result).Message);
		}


		private bool ResultsMatch(List<KeyValuePair<string, int>> expected, List<KeyValuePair<string, int>> actual)
		{
			for (int i = 0; i < expected.Count; i++)
			{
				if (expected[i].Key != actual[i].Key)
				{
					return false;
				}

				if (expected[i].Value != actual[i].Value)
				{
					return false;
				}
			}

			return true;
		}
	}
}