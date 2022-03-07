using System;
using kCura.IntegrationPoints.Core.Models;
using NUnit.Framework;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Synchronizers.RDO;
using NSubstitute;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Tests.Validation
{
	[TestFixture, Category("Unit")]
	public class IntegrationPointPermissionValidatorTests
	{
		private IIntegrationPointSerializer _serializer;
		private IntegrationPointModelBase _model;
		private int _artifactId = 123456;
		private SourceProvider _sourceProvider;
		private DestinationProvider _destinationProvider;
		private IntegrationPointType _integrationPointType;
		private Guid _objectTypeGuid = Guid.Empty;
		private int _userId = 101;

		[SetUp]
		public void SetUp()
		{
			_serializer = Substitute.For<IIntegrationPointSerializer>();
			_model = new IntegrationPointModelBase() { Destination = $"{{ \"artifactTypeID\": { _artifactId } }}" };
			_serializer.Deserialize<ImportSettings>(_model.Destination).Returns(new ImportSettings() { ArtifactTypeId = _artifactId });
			_sourceProvider = new SourceProvider() { ArtifactId = 1000, Identifier = Guid.NewGuid().ToString() };
			_destinationProvider = new DestinationProvider() { ArtifactId = 2000, Identifier = Guid.NewGuid().ToString() };
			_integrationPointType = new IntegrationPointType() { Identifier = Guid.NewGuid().ToString() };
		}

		[Test]
		public void TestIntegrationPointValidatorGetsCalled()
		{
			//arrange
			var validator = Substitute.For<IPermissionValidator>();
			validator.Key.Returns(Constants.IntegrationPoints.Validation.INTEGRATION_POINT);

			var permissionValidator = new IntegrationPointPermissionValidator(new[] { validator }, _serializer);

			//act
			permissionValidator.Validate(_model, _sourceProvider, _destinationProvider, _integrationPointType, _objectTypeGuid, _userId);

			//assert
			validator.Received(1).Validate(Arg.Any<object>());
		}

		[Test]
		public void TestImportValidatorGetsCalled()
		{
			//arrange
			var validator = Substitute.For<IPermissionValidator>();
			validator.Key.Returns(Constants.IntegrationPoints.IntegrationPointTypes.ImportGuid.ToString());

			var permissionValidator = new IntegrationPointPermissionValidator(new[] { validator }, _serializer);

			//act
			_integrationPointType.Identifier = Constants.IntegrationPoints.IntegrationPointTypes.ImportGuid.ToString();
			permissionValidator.Validate(_model, _sourceProvider, _destinationProvider, _integrationPointType, _objectTypeGuid, _userId);

			//assert
			validator.Received(1).Validate(Arg.Any<object>());
		}

		[Test]
		public void TestExportValidatorDoesNotGetCalled()
		{
			//arrange
			var validator = Substitute.For<IPermissionValidator>();
			validator.Key.Returns(Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid.ToString());

			var permissionValidator = new IntegrationPointPermissionValidator(new[] { validator }, _serializer);

			//act
			_integrationPointType.Identifier = Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid.ToString();
			permissionValidator.Validate(_model, _sourceProvider, _destinationProvider, _integrationPointType, _objectTypeGuid, _userId);

			//assert
			validator.DidNotReceive().Validate(Arg.Any<object>());
		}

		[Test]
		public void TestSpecificValidatorGetsCalled()
		{
			//arrange
			var validator = Substitute.For<IPermissionValidator>();
			var sourceGuid = Guid.NewGuid();
			var destinationGuid = Guid.NewGuid();

			validator.Key.Returns(IntegrationPointPermissionValidator.GetProviderValidatorKey(sourceGuid.ToString(), destinationGuid.ToString()));

			var permissionValidator = new IntegrationPointPermissionValidator(new[] { validator }, _serializer);

			//act
			_sourceProvider.Identifier = sourceGuid.ToString();
			_destinationProvider.Identifier = destinationGuid.ToString();
			permissionValidator.Validate(_model, _sourceProvider, _destinationProvider, _integrationPointType, _objectTypeGuid, _userId);

			//assert
			validator.Received(1).Validate(Arg.Any<object>());
		}

		[Test]
		public void TestIntegrationPointValidatorAndSaveValidatorGetCalled()
		{
			//arrange
			var validator = Substitute.For<IPermissionValidator>();
			validator.Key.Returns(Constants.IntegrationPoints.Validation.INTEGRATION_POINT);

			var saveValidator = Substitute.For<IPermissionValidator>();
			saveValidator.Key.Returns(Constants.IntegrationPoints.Validation.SAVE);

			var permissionValidator = new IntegrationPointPermissionValidator(new[] { validator, saveValidator }, _serializer);

			//act
			permissionValidator.ValidateSave(_model, _sourceProvider, _destinationProvider, _integrationPointType, _objectTypeGuid, _userId);

			//assert
			validator.Received(1).Validate(Arg.Any<object>());
			saveValidator.Received(1).Validate(Arg.Any<object>());
		}

		[Test]
		public void TestViewErrorsValidatorGetsCalled()
		{
			//arrange
			var validator = Substitute.For<IPermissionValidator>();
			validator.Key.Returns(Constants.IntegrationPoints.Validation.VIEW_ERRORS);

			var permissionValidator = new IntegrationPointPermissionValidator(new[] { validator }, _serializer);

			//act
			permissionValidator.ValidateViewErrors(1);

			//assert
			validator.Received(1).Validate(Arg.Any<object>());
		}


		[Test]
		public void TestStopValidatorGetsCalled()
		{
			//arrange
			var validator = Substitute.For<IPermissionValidator>();
			validator.Key.Returns(Constants.IntegrationPoints.Validation.STOP);

			var permissionValidator = new IntegrationPointPermissionValidator(new[] { validator }, _serializer);

			//act
			permissionValidator.ValidateStop(_model, _sourceProvider, _destinationProvider, _integrationPointType, _objectTypeGuid, _userId);

			//assert
			validator.Received(1).Validate(Arg.Any<object>());
		}
	}
}
