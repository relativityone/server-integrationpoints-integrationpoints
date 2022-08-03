using System;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Web.IntegrationPointsServices.Logging;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Web.Tests.IntegrationPointsServices.Logging
{
    [TestFixture, Category("Unit")]
    public class WebCorrelationContextProviderTests : TestBase
    {
        private WebActionContextProvider _subjectUnderTests;
        private WebActionContext _webActionContext;

        private ICacheHolder _cacheHolder;
        private const int _USER_ID = 1234;
        private const string _ACTION_NAME = "ActionName";

        private static readonly Guid _actionGuid = Guid.NewGuid();

        public override void SetUp()
        {
            _cacheHolder = Substitute.For<ICacheHolder>();

            _subjectUnderTests = new WebActionContextProvider(_cacheHolder);
            _webActionContext = new WebActionContext(_ACTION_NAME, _actionGuid);
        }

        [Test]
        // Run Job Action Test Cases
        [TestCase("https://test.kcura.corp/Relativity/CustomPages/DCF6E9D1-22B6-4DA3-98F6-41381E93C30C/1018658/api/Job", true, WebActionContextProvider.JOB_RUN_ACTION)]
        [TestCase("http://test.kcura.corp/Relativity/CustomPages/DCF6E9D1-22B6-4DA3-98F6-41381E93C30C/1018658/api/Job", false, _ACTION_NAME)]
        [TestCase("https://test.kcura.corp/Relativity2/CustomPages/DCF6E9D1-22B6-4DA3-98F6-41381E93C30C/1018658/api/Job", false, _ACTION_NAME)]
        [TestCase("https://test.kcura.corp/Relativity/CustomPages/DCF6E9D1-22B6-4DA3-98F6-41381E93C30C/1018658/Job", false, _ACTION_NAME)]
        [TestCase("https://test.kcura.corp/Relativity/CustomPages/ACF6E9D1-22B6-4DA3-98F6-41381E93C30C/1018658/api/Job", false, _ACTION_NAME)]
        [TestCase("https://test.kcura.corp/Relativity/CustomPages/DCF6E9D1-22B6-4DA3-98F6-41381E93C30C/1018658/api/Job2", false, _ACTION_NAME)]
        // New Job Action Test Cases
        [TestCase("https://test.relativity.corp/Relativity/CustomPages/DCF6E9D1-22B6-4DA3-98F6-41381E93C30C/1018658/api/IntegrationPointsAPI/0?_=1510730418617", true, WebActionContextProvider.JOB_NEW_ACTION)]
        [TestCase("https://test.kcura.corp/Relativity/CustomPages/DCF6E9D1-22B6-4DA3-98F6-41381E93C30C/1018658/api/IntegrationPointsAPI/1040486", false, _ACTION_NAME)]
        [TestCase("https://test.relativity.corp/Relativity/CustomPages/DCF6E9D1-22B6-4DA3-98F6-41381E93C30C/1018658/api/IntegrationPointsAPI/0?_=", false, _ACTION_NAME)]
        [TestCase("https://test.relativity.corp/Relativity/CustomPages/DCF6E9D1-22B6-4DA3-98F6-41381E93C30C/1018658/api/IntegrationPointsAPI/?_=1510730418617", false, _ACTION_NAME)]
        [TestCase("https://test.relativity.corp/Relativity/CustomPages/DCF6E9D1-22B6-4DA3-98F6-41381E93C30C/1018658/api/IntegrationPointsAPI/0?=1510730418617", false, _ACTION_NAME)]
        // Edit Job Action Test Cases
        [TestCase("https://test.kcura.corp/Relativity/CustomPages/DCF6E9D1-22B6-4DA3-98F6-41381E93C30C/1018658/api/IntegrationPointsAPI/1040486?_=1234", true, WebActionContextProvider.JOB_EDIT_ACTION)]
        [TestCase("https://test.kcura.corp/Relativity/CustomPages/DCF6E9D1-22B6-4DA3-98F6-41381E93C30C/1018658/api/IntegrationPointsAPI/1040486?_=", false, _ACTION_NAME)]
        [TestCase("https://test.kcura.corp/Relativity/CustomPages/DCF6E9D1-22B6-4DA3-98F6-41381E93C30C/1018658/api/IntegrationPointsAPI/?_=122", false, _ACTION_NAME)]
        [TestCase("https://test.kcura.corp/Relativity/CustomPages/DCF6E9D1-22B6-4DA3-98F6-41381E93C30C/1018658/api/IntegrationPointsAPI/1040486?=123", false, _ACTION_NAME)]
        [TestCase("https://test.kcura.corp/Relativity/CustomPages/DCF6E9D1-22B6-4DA3-98F6-41381E93C30C/1018658/api/IntegrationPointsAPI/1040486=", false, _ACTION_NAME)]
        // Sava as Profile Test Cases
        [TestCase("https://test.kcura.corp/Relativity/CustomPages/DCF6E9D1-22B6-4DA3-98F6-41381E93C30C/1018658/api/IntegrationPointProfilesAPI/SaveAsProfile/1040489/LDAP Profile", true, WebActionContextProvider.JOB_SAVE_AS_PROFILE_ACTION)]
        [TestCase("https://test.kcura.corp/Relativity/CustomPages/DCF6E9D1-22B6-4DA3-98F6-41381E93C30C/1018658/api/IntegrationPointProfilesAPI/SaveAsProfil", false, _ACTION_NAME)]
        [TestCase("https://test.kcura.corp/Relativity/CustomPages/DCF6E9D1-22B6-4DA3-98F6-41381E93C30C/1018658/api/IntegrationPointProfilesAPI", false, _ACTION_NAME)]
        // New Profile Action Test Cases
        [TestCase("https://test.relativity.corp/Relativity/CustomPages/DCF6E9D1-22B6-4DA3-98F6-41381E93C30C/1018658/api/IntegrationPointProfilesAPI/0?_=1510730418617", true, WebActionContextProvider.JOB_NEW_PROFILE_ACTION)]
        [TestCase("https://test.kcura.corp/Relativity/CustomPages/DCF6E9D1-22B6-4DA3-98F6-41381E93C30C/1018658/api/IntegrationPointProfilesAPI/1040486", false, _ACTION_NAME)]
        [TestCase("https://test.relativity.corp/Relativity/CustomPages/DCF6E9D1-22B6-4DA3-98F6-41381E93C30C/1018658/api/IntegrationPointProfilesAPI/0?_=", false, _ACTION_NAME)]
        [TestCase("https://test.relativity.corp/Relativity/CustomPages/DCF6E9D1-22B6-4DA3-98F6-41381E93C30C/1018658/api/IntegrationPointProfilesAPI/?_=1510730418617", false, _ACTION_NAME)]
        [TestCase("https://test.relativity.corp/Relativity/CustomPages/DCF6E9D1-22B6-4DA3-98F6-41381E93C30C/1018658/api/IntegrationPointProfilesAPI/0?=1510730418617", false, _ACTION_NAME)]
        // Edit Profile Action Test Cases
        [TestCase("https://test.kcura.corp/Relativity/CustomPages/DCF6E9D1-22B6-4DA3-98F6-41381E93C30C/1018658/api/IntegrationPointProfilesAPI/1040486?_=1234", true, WebActionContextProvider.JOB_EDIT_PROFILE_ACTION)]
        [TestCase("https://test.kcura.corp/Relativity/CustomPages/DCF6E9D1-22B6-4DA3-98F6-41381E93C30C/1018658/api/IntegrationPointProfilesAPI/1040486?_=", false, _ACTION_NAME)]
        [TestCase("https://test.kcura.corp/Relativity/CustomPages/DCF6E9D1-22B6-4DA3-98F6-41381E93C30C/1018658/api/IntegrationPointProfilesAPI/?_=122", false, _ACTION_NAME)]
        [TestCase("https://test.kcura.corp/Relativity/CustomPages/DCF6E9D1-22B6-4DA3-98F6-41381E93C30C/1018658/api/IntegrationPointProfilesAPI/1040486?=123", false, _ACTION_NAME)]
        [TestCase("https://test.kcura.corp/Relativity/CustomPages/DCF6E9D1-22B6-4DA3-98F6-41381E93C30C/1018658/api/IntegrationPointProfilesAPI/1040486=", false, _ACTION_NAME)]
        public void ItShouldValidateRunJobAction(string url, bool result, string actionName)
        {
            // Arrange
            _cacheHolder.GetObject<WebActionContext>(_USER_ID.ToString()).Returns(_webActionContext);

            // Act
            var webActionContext = _subjectUnderTests.GetDetails(url, _USER_ID);

            // Assert
            Assert.That(webActionContext.ActionName, Is.EqualTo(actionName));
            Assert.That(webActionContext.ActionGuid, result ? Is.Not.EqualTo(_actionGuid) : Is.EqualTo(_actionGuid));

            if (!result)
            {
                _cacheHolder.Received().GetObject<WebActionContext>(_USER_ID.ToString());
            }
        }
    }
}
