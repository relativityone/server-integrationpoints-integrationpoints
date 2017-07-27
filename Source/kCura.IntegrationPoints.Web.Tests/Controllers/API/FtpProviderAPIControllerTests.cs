using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Results;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Provider;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Core.Services.Synchronizer;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.FtpProvider.Helpers.Interfaces;
using kCura.IntegrationPoints.FtpProvider.Helpers.Models;
using kCura.IntegrationPoints.Security;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.IntegrationPoints.Web.Controllers.API;
using kCura.IntegrationPoints.Web.Models;
using Newtonsoft.Json;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Relativity.API;
using Relativity.Telemetry.Services.Metrics;

namespace kCura.IntegrationPoints.Web.Tests.Controllers.API
{
	[TestFixture]
	public class FtpProviderAPIControllerTests : TestBase
	{
		private FtpProviderAPIController _instance;
		private IEncryptionManager _securityManager;
		private ISettingsManager _settingsManager;
		private IDataProviderFactory _providerFactory;
		private IHelper _helper;
		private ISerializer _serializer;
		
		[SetUp]
		public override void SetUp()
		{
			_securityManager = Substitute.For<IEncryptionManager>();
			_settingsManager = Substitute.For<ISettingsManager>();
			_providerFactory = Substitute.For<IDataProviderFactory>();
			_serializer = Substitute.For<ISerializer>();
			_helper = Substitute.For<IHelper>();

			_instance =
				new FtpProviderAPIController(_securityManager, _settingsManager, _providerFactory, _helper, _serializer)
				{
					Request = new HttpRequestMessage()
				};

			_instance.Request.SetConfiguration(new HttpConfiguration());
		}

		[TestCase("test")]
		[TestCase(123)]
		public void ItShouldEncryptMessage(object message)
		{
			//Arrange
			_securityManager.Encrypt(message.ToString()).Returns(message.ToString());

			//Act
			IHttpActionResult actualResult = _instance.Encrypt(message);

			//Assert
			Assert.AreEqual(typeof(OkNegotiatedContentResult<string>), actualResult.GetType());
			Assert.AreEqual(message.ToString(), ((OkNegotiatedContentResult<string>)actualResult).Content);
		}

		[TestCase(null)]
		public void ItShouldReturnEmptyStringInsteadOfEncryptedMessage(object message)
		{
			//Act
			IHttpActionResult actualResult = _instance.Encrypt(message);

			//Assert
			Assert.AreEqual(typeof(OkNegotiatedContentResult<string>), actualResult.GetType());
			Assert.AreEqual(string.Empty, ((OkNegotiatedContentResult<string>)actualResult).Content);
		}

		[TestCase("test")]
		public void ItShouldDecryptMessage(string message)
		{
			//Arrange
			_securityManager.Decrypt(message).Returns(message);

			//Act
			IHttpActionResult actualResult = _instance.Decrypt(message);

			//Assert
			Assert.AreEqual(typeof(OkNegotiatedContentResult<string>), actualResult.GetType());
			Assert.AreEqual(message, ((OkNegotiatedContentResult<string>)actualResult).Content);
		}

		[TestCase("some data")]
		public void ItShouldGetColumnList([FromBody] object data)
		{
			//Arrange
			var fields = new List<FieldEntry>() {new FieldEntry() {DisplayName = "A"}, new FieldEntry() {DisplayName = "B"}};
			var encryptedData = "Encrypted";
			_securityManager.Encrypt(data.ToString()).Returns(encryptedData);

			var ftpProvider = Substitute.For<IDataSourceProvider>();
			ftpProvider.GetFields(encryptedData).Returns(fields);
			_providerFactory.GetDataProvider(Arg.Any<Guid>(), Arg.Any<Guid>(), _helper).Returns(ftpProvider);

			//Act
			IHttpActionResult actualResult = _instance.GetColumnList(data);

			//Assert
			Assert.AreEqual(typeof(OkNegotiatedContentResult<List<FieldEntry>>), actualResult.GetType());
			Assert.AreEqual(fields, ((OkNegotiatedContentResult<List<FieldEntry>>)actualResult).Content);
		}

		[TestCase("some data")]
		public void ItShouldGetViewFields([FromBody] object data)
		{
			//Arrange
			var settings = new Settings() {_filename = "SettigsFileName", Host = "HostName"};
			_settingsManager.ConvertFromEncryptedString(data.ToString()).Returns(settings);
			var expectedModel = new FtpProviderSummaryPageSettingsModel(settings);
			string expectedSerializedModel = JsonConvert.SerializeObject(expectedModel);

			_serializer.Serialize(Arg.Any<FtpProviderSummaryPageSettingsModel>()).Returns(expectedSerializedModel);

			//Act
			HttpResponseMessage response = _instance.GetViewFields(data);

			//Assert
			Assert.IsNotNull(response, "Response should not be null");
			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "HttpStatusCode should be OK");
			Assert.AreEqual(expectedSerializedModel, JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result), "The HttpContent should be as expected");
		}
	}
}
