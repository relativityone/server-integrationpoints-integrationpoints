﻿using FluentAssertions;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Validation.Parts;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;
using System.Collections.Generic;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core.Extensions.Moq;
using kCura.IntegrationPoints.Core.Factories;
using Relativity.Services.Interfaces.Group;

namespace kCura.IntegrationPoints.Core.Tests.Validation
{
	[TestFixture, Category("Unit")]
	public class NativeCopyLinksValidatorTests
	{
		private NativeCopyLinksValidator _sut;

		private Mock<ISerializer> _serializerFake;
		private Mock<IAPILog> _loggerFake;
		private Mock<IHelper> _helperFake;
		private Mock<IServicesMgr> _servicesMgrFake;
		private Mock<IGroupManager> _groupManager;
		private Mock<IInstanceSettingsManager> _instanceSettingsFake;
		private Mock<IManagerFactory> _managerFactoryFake;

		private const int _SOURCE_WORKSPACE_ID = 10000;
		private const int _ADMIN_GROUP_ID = 100;
		private const int _USER_IS_ADMIN_ID = 1;

		[SetUp]
		public void SetUp()
		{
			_loggerFake = new Mock<IAPILog>();
			_loggerFake.SetupLog();

			_groupManager = new Mock<IGroupManager>();

			_servicesMgrFake = new Mock<IServicesMgr>();
			_servicesMgrFake.Setup(m => m.CreateProxy<IGroupManager>(ExecutionIdentity.System)).Returns(_groupManager.Object); 

			_helperFake = new Mock<IHelper>();
			_helperFake.Setup(m => m.GetServicesManager()).Returns(_servicesMgrFake.Object);

			_serializerFake = new Mock<ISerializer>();
			_instanceSettingsFake = new Mock<IInstanceSettingsManager>();

			_managerFactoryFake = new Mock<IManagerFactory>();
			_managerFactoryFake.Setup(m => m.CreateInstanceSettingsManager()).Returns(_instanceSettingsFake.Object);

			_sut = new NativeCopyLinksValidator(_loggerFake.Object, _helperFake.Object, _serializerFake.Object, _managerFactoryFake.Object);
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

			List<RelativityObjectSlim> groups = userIsAdmin
				? new List<RelativityObjectSlim> { new RelativityObjectSlim() { ArtifactID = _ADMIN_GROUP_ID } }
				: new List<RelativityObjectSlim>();
			QueryResultSlim queryResultSlim = new QueryResultSlim { Objects = groups };

			var importSettings = new ImportSettings() { ImportNativeFileCopyMode = fileCopyMode };

			_instanceSettingsFake.Setup(s => s.RetrieveRestrictReferentialFileLinksOnImport())
				.Returns(restrictReferentialFileLinksOnImport);
			_serializerFake.Setup(s => s.Deserialize<ImportSettings>(It.IsAny<string>()))
				.Returns(importSettings);
			_groupManager.Setup(p => p.QueryGroupsByUserAsync(It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>(), validationModel.UserId))
				.Returns(Task.FromResult(queryResultSlim));

			return validationModel;
		}
	}
}
