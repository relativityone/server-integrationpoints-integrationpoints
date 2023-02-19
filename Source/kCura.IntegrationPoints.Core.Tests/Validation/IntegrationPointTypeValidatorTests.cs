using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Core.Validation.Parts;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using Relativity.IntegrationPoints.FieldsMapping.Models;

namespace kCura.IntegrationPoints.Core.Tests.Validation
{
    [TestFixture, Category("Unit")]
    public class IntegrationPointTypeValidatorTests
    {
        private IRelativityObjectManager _objectManager;
        private const int _INTEGRATION_POINT_TYPE = 0;
        private const string _LOAD_FILE_SOURCE_PROVIDE_IDENTIFIER = "548F0873-8E5E-4DA6-9F27-5F9CDA764636";
        private const string _FTP_SOURCE_PROVIDE_IDENTIFIER = "85120bc8-b2b9-4f05-99e9-de37bb6c0e15";
        private const string _LDAP_SOURCE_PROVIDE_IDENTIFIER = "5bf1f2c2-9670-4d6e-a3e9-dbc83db6c232";

        public enum IpType
        {
            Import,
            Export
        }

        [SetUp]
        public void SetUp()
        {
            _objectManager = Substitute.For<IRelativityObjectManager>();
        }

        [Test]
        [TestCase(IntegrationPoints.Domain.Constants.RELATIVITY_PROVIDER_GUID, IpType.Export)]
        [TestCase(_LOAD_FILE_SOURCE_PROVIDE_IDENTIFIER, IpType.Import)]
        [TestCase(_FTP_SOURCE_PROVIDE_IDENTIFIER, IpType.Import)]
        [TestCase(_LDAP_SOURCE_PROVIDE_IDENTIFIER, IpType.Import)]
        public void ValidateIntegrationPointTypeValid(string sourceProviderId, IpType ipType)
        {
            // Arrange
            MockIpTypeId(ipType);
            IAPILog logger = Substitute.For<IAPILog>();
            var validator = new IntegrationPointTypeValidator(_objectManager, logger);
            var ipModel = GetProviderValidationModelObject(sourceProviderId);

            // Act
            ValidationResult result = validator.Validate(ipModel);

            // Assert
            Assert.IsTrue(result.IsValid);
            Assert.IsNull(result.MessageTexts.FirstOrDefault());
        }

        [TestCase(IntegrationPoints.Domain.Constants.RELATIVITY_PROVIDER_GUID, IpType.Import)]
        [TestCase(_LOAD_FILE_SOURCE_PROVIDE_IDENTIFIER, IpType.Export)]
        [TestCase(_FTP_SOURCE_PROVIDE_IDENTIFIER, IpType.Export)]
        [TestCase(_LDAP_SOURCE_PROVIDE_IDENTIFIER, IpType.Export)]
        public void ValidateIntegrationPointTypeInvalid(string sourceProviderId, IpType ipType)
        {
            // Arrange
            MockIpTypeId(ipType);
            IAPILog logger = Substitute.For<IAPILog>();
            var validator = new IntegrationPointTypeValidator(_objectManager, logger);
            var ipModel = GetProviderValidationModelObject(sourceProviderId);

            // Act
            ValidationResult result = validator.Validate(ipModel);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.That(result.MessageTexts.Contains(IntegrationPointProviderValidationMessages.ERROR_INTEGRATION_POINT_TYPE_INVALID));
        }

        public void ValidateIntegrationPointTypeMissing()
        {
            // Arrange
            IntegrationPointType ipTypeObject = null;
            _objectManager.Read<IntegrationPointType>(Arg.Any<int>()).Returns(ipTypeObject);
            IAPILog logger = Substitute.For<IAPILog>();
            var validator = new IntegrationPointTypeValidator(_objectManager, logger);
            var ipModel = GetProviderValidationModelObject(IntegrationPoints.Domain.Constants.RELATIVITY_PROVIDER_GUID);

            // Act
            ValidationResult result = validator.Validate(ipModel);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.That(result.MessageTexts.Contains(IntegrationPointProviderValidationMessages.ERROR_INTEGRATION_POINT_TYPE_INVALID));
        }

        private IntegrationPointProviderValidationModel GetProviderValidationModelObject(string sourceProviderId)
        {
            return new IntegrationPointProviderValidationModel()
            {
                FieldsMap = new List<FieldMap>(),
                SourceProviderIdentifier = sourceProviderId,
                Type = _INTEGRATION_POINT_TYPE
            };
        }

        private void MockIpTypeId(IpType ipType)
        {
            var integrationPointType = new IntegrationPointType()
            {
                Identifier = GetIpTypeId(ipType)
            };
            _objectManager.Read<IntegrationPointType>(Arg.Any<int>()).Returns(integrationPointType);
        }

        private string GetIpTypeId(IpType ipType)
        {
            return ipType == IpType.Export ?
                Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid.ToString() :
                Constants.IntegrationPoints.IntegrationPointTypes.ImportGuid.ToString();
        }
    }
}
