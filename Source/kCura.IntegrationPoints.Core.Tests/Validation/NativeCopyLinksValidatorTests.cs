using FluentAssertions;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Validation.Parts;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core.Extensions.Moq;
using kCura.IntegrationPoints.Core.Factories;

namespace kCura.IntegrationPoints.Core.Tests.Validation
{
	[TestFixture]
	public class NativeCopyLinksValidatorTests
	{
		private NativeCopyLinksValidator _sut;

		private Mock<ISerializer> _serializerFake;
		private Mock<IAPILog> _loggerFake;
		private Mock<IInstanceSettingsManager> _instanceSettingsFake;
		private Mock<IRelativityObjectManagerFactory> _relativityObjectManagerFactoryFake;
		private Mock<IPermissionManager> _permissionManagerFake;
		private Mock<IRelativityObjectManager> _objectManagerFake;
		private Mock<IManagerFactory> _managerFactoryFake;

		private const int _SOURCE_WORKSPACE_ID = 10000;
		private const int _ADMIN_GROUP_ID = 100;
		private const int _USER_IS_ADMIN_ID = 1;

		private readonly List<RelativityObject> _ADMIN_GROUPS = new List<RelativityObject>()
		{
			new RelativityObject() {ArtifactID = _ADMIN_GROUP_ID}
		};

		[SetUp]
		public void SetUp()
		{
			_loggerFake = new Mock<IAPILog>();
			_loggerFake.SetupLog();

			_serializerFake = new Mock<ISerializer>();
			_instanceSettingsFake = new Mock<IInstanceSettingsManager>();
			_objectManagerFake = new Mock<IRelativityObjectManager>();
			_objectManagerFake.Setup(m => m.Query(It.IsAny<QueryRequest>(), It.IsAny<ExecutionIdentity>()))
				.Returns(_ADMIN_GROUPS);

			_relativityObjectManagerFactoryFake = new Mock<IRelativityObjectManagerFactory>();
			_relativityObjectManagerFactoryFake.Setup(m => m.CreateRelativityObjectManager(It.IsAny<int>()))
				.Returns(_objectManagerFake.Object);

			_permissionManagerFake = new Mock<IPermissionManager>();

			_managerFactoryFake = new Mock<IManagerFactory>();
			_managerFactoryFake.Setup(m => m.CreateInstanceSettingsManager()).Returns(_instanceSettingsFake.Object);
			_managerFactoryFake.Setup(m => m.CreatePermissionManager()).Returns(_permissionManagerFake.Object);

			_sut = new NativeCopyLinksValidator(_loggerFake.Object, _serializerFake.Object, _relativityObjectManagerFactoryFake.Object, _managerFactoryFake.Object);
		}

		[Test]
		public void Validate_ShouldHandleValidConfiguration_WhenConditionsAreMet()
		{
			//arrange
			var userIsAdmin = true;
			var nativeFileCopyMode = ImportNativeFileCopyModeEnum.SetFileLinks;
			var restrictReferentialFileLinksOnImport = true;
			var validationModel = SetupValidator(userIsAdmin, nativeFileCopyMode, restrictReferentialFileLinksOnImport);

			//act
			ValidationResult result = _sut.Validate(validationModel);

			//assert
			result.IsValid.Should().BeTrue();
			result.Messages.Should().BeEmpty();
		}

		[Test]
		public void Validate_ShouldHandleInvalidConfiguration_WhenUserIsNonAdmin()
		{
			//arrange
			var userIsAdmin = false;
			var nativeFileCopyMode = ImportNativeFileCopyModeEnum.SetFileLinks;
			var restrictReferentialFileLinksOnImport = true;
			var validationModel = SetupValidator(userIsAdmin, nativeFileCopyMode, restrictReferentialFileLinksOnImport);

			//act
			ValidationResult result = _sut.Validate(validationModel);

			//assert
			result.IsValid.Should().BeFalse();
			result.Messages.Should().NotBeEmpty();
		}

		[Test]
		public void Validate_ShouldSkipValidationIndependentOfUser_WhenResponsibleInstanceSettingIsFalse()
		{
			//arrange
			var userIsAdmin = false;
			var nativeFileCopyMode = ImportNativeFileCopyModeEnum.SetFileLinks;
			var restrictReferentialFileLinksOnImport = false;
			var validationModel = SetupValidator(userIsAdmin, nativeFileCopyMode, restrictReferentialFileLinksOnImport);

			//act
			ValidationResult result = _sut.Validate(validationModel);

			//assert
			result.IsValid.Should().BeTrue();
			result.Messages.Should().BeEmpty();
		}

		[Test]
		public void Validate_ShouldSkipValidation_WhenNativeCopyModeIsNotFileLinks()
		{
			//arrange
			var userIsAdmin = false;
			var nativeFileCopyMode = ImportNativeFileCopyModeEnum.CopyFiles;
			var restrictReferentialFileLinksOnImport = true;
			var validationModel = SetupValidator(userIsAdmin, nativeFileCopyMode, restrictReferentialFileLinksOnImport);

			//act
			ValidationResult result = _sut.Validate(validationModel);

			//assert
			result.IsValid.Should().BeTrue();
			result.Messages.Should().BeEmpty();
		}

		private IntegrationPointProviderValidationModel SetupValidator(
			bool userIsAdmin, 
			ImportNativeFileCopyModeEnum fileCopyMode, 
			bool restrictReferentialFileLinksOnImport)
		{
			var validationModel = new IntegrationPointProviderValidationModel()
			{
				DestinationConfiguration = $"{{ \"importNativeFileCopyMode\": \"{fileCopyMode.ToString()}\", \"caseArtifactId\": \"{_SOURCE_WORKSPACE_ID}\" }}",
				UserId = _USER_IS_ADMIN_ID
			};

			var importSettings = new ImportSettings() { ImportNativeFileCopyMode = fileCopyMode };

			_instanceSettingsFake.Setup(s => s.RetrieveRestrictReferentialFileLinksOnImport())
				.Returns(restrictReferentialFileLinksOnImport);
			_serializerFake.Setup(s => s.Deserialize<ImportSettings>(It.IsAny<string>()))
				.Returns(importSettings);
			_permissionManagerFake.Setup(p => p.UserBelongsToGroup(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
				.Returns(userIsAdmin);

			return validationModel;
		}
	}
}
