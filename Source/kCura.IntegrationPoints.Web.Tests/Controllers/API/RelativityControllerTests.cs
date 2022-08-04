using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Results;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Interfaces.TextSanitizer;
using kCura.IntegrationPoints.Web.Controllers.API;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Web.Tests.Controllers.API
{
    [TestFixture, Category("Unit")]
    public class RelativityControllerTests : TestBase
    {
        private RelativityController _instance;
        private const int WORKSPACE_ARTIFACT_ID = 10293;
        private const int SAVED_SEARCH_ID = 123434;
        private ITextSanitizer _stringSanitizer;

        [SetUp]
        public override void SetUp()
        {
            _stringSanitizer = Substitute.For<ITextSanitizer>();
            _stringSanitizer
                .Sanitize(Arg.Any<string>())
                .Returns(x => new SanitizationResult(sanitizedText: x.Arg<string>(), hasErrors: false));
            _instance = new RelativityController(_stringSanitizer);
        }

        [Test]
        public void GetViewFields_SameTypeParams_ReturnsCorrectResult()
        {
            // Arrange
            string data = Newtonsoft.Json.JsonConvert.SerializeObject(new
            {
                TargetWorkspaceID = WORKSPACE_ARTIFACT_ID,
                SavedSearchID = SAVED_SEARCH_ID
            });

            var expectedResult = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>(
                    "Target Workspace ID", WORKSPACE_ARTIFACT_ID),
                new KeyValuePair<string, object>("Saved Search ID",
                    SAVED_SEARCH_ID)
            };

            // Act
            IHttpActionResult result = _instance.GetViewFields(data);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<OkNegotiatedContentResult<List<KeyValuePair<string, object>>>>(result);
            Assert.IsTrue(this.ResultsMatch(expectedResult,
                ((OkNegotiatedContentResult<List<KeyValuePair<string, object>>>)result).Content));
        }

        [Test]
        public void GetViewFields_MixmatchedTypeParams_ReturnsCorrectResult()
        {
            // Arrange
            string data = Newtonsoft.Json.JsonConvert.SerializeObject(new
            {
                TargetWorkspaceID = WORKSPACE_ARTIFACT_ID,
                Test = "wooplah"
            });

            var expectedResult = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>(
                    "Target Workspace ID", WORKSPACE_ARTIFACT_ID),
                new KeyValuePair<string, object>("Test", "wooplah")
            };

            // Act
            IHttpActionResult result = _instance.GetViewFields(data);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<OkNegotiatedContentResult<List<KeyValuePair<string, object>>>>(result);
            Assert.IsTrue(this.ResultsMatch(expectedResult,
                ((OkNegotiatedContentResult<List<KeyValuePair<string, object>>>)result).Content));
        }

        private bool ResultsMatch(List<KeyValuePair<string, object>> expected, List<KeyValuePair<string, object>> actual)
        {
            for (int i = 0; i < expected.Count; i++)
            {
                if (expected[i].Key != actual[i].Key)
                {
                    return false;
                }

                if (expected[i].Value.ToString() != actual[i].Value.ToString())
                {
                    return false;
                }
            }

            return true;
        }
    }
}