using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Web.Controllers.API;
using kCura.IntegrationPoints.Web.DataStructures;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Web.Tests.Controllers.API
{
    [TestFixture, Category("Unit")]
    internal class GetAvailableEncodingsControllerTests : TestBase
    {
        #region Fields

        private GetAvailableEncodingsController _subjectUnderTest;
        private EncodingInfo[] _allEncodings;

        #endregion //Fields

        [SetUp]
        public override void SetUp()
        {
            _allEncodings = Encoding.GetEncodings();

            _subjectUnderTest = new GetAvailableEncodingsController()
            {
                Request = new HttpRequestMessage()
            };

            _subjectUnderTest.Request.SetConfiguration(new HttpConfiguration());
        }

        [Test]
        public void ItShouldGetAllEncodingsOrderedByOrdinalDisplayName()
        {
            // Act
            HttpResponseMessage httpResponseMessage = _subjectUnderTest.Get();

            // Assert
            List<AvailableEncodingInfo> retValue;
            httpResponseMessage.TryGetContentValue(out retValue);

            Assert.That(httpResponseMessage.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            Assert.That(retValue.Count, Is.EqualTo(_allEncodings.Length));
            Assert.That(retValue.TrueForAll(encodingInfo =>
                _allEncodings.Any(e => e.DisplayName == encodingInfo.DisplayName && e.Name == encodingInfo.Name)));

            retValue.Should().BeInAscendingOrder( e => e.DisplayName, StringComparer.Ordinal );
        }
    }
}
