﻿
using System;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Exceptions;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Models;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Validation
{
	public class ValidationExecutorTests : TestBase
	{
		#region Fields

		private DestinationProvider _destinationProvider;
		private IIntegrationPointPermissionValidator _permissionValidatorMock;
		private IIntegrationPointProviderValidator _providerValidatorMock;
		private IntegrationPointModel _model;
		private IntegrationPointType _integrationPointType;
		private SourceProvider _sourceProvider;

		private ValidationContext _validationContex;

		private ValidationExecutor _subjectUnderTest;
		private const int _USER_ID = 1;
		private readonly string _OBJECT_TYPE_GUID = Guid.NewGuid().ToString();


		#endregion // Fields

		#region Setup

		public override void SetUp()
		{
			_providerValidatorMock = Substitute.For<IIntegrationPointProviderValidator>();
			_permissionValidatorMock = Substitute.For<IIntegrationPointPermissionValidator>();

			_subjectUnderTest = new ValidationExecutor(_providerValidatorMock, _permissionValidatorMock);

			_destinationProvider = new DestinationProvider();
			_sourceProvider = new SourceProvider();
			_model = new IntegrationPointModel();
			_integrationPointType = new IntegrationPointType();

			_validationContex = new ValidationContext()
			{
				DestinationProvider = _destinationProvider,
				SourceProvider = _sourceProvider,
				IntegrationPointType = _integrationPointType,
				Model = _model,
				ObjectTypeGuid = _OBJECT_TYPE_GUID,
				UserId = _USER_ID
			};
		}

		#endregion // Setup

		#region Tests

		[Test]
		public void ItShouldThrowValidationExceptionOnNotDefinedUserid()
		{
			// ARRANGE

			_permissionValidatorMock.Validate(_model, _sourceProvider,
				_destinationProvider, _integrationPointType, _OBJECT_TYPE_GUID).Returns(new ValidationResult());

			_permissionValidatorMock.ValidateSave(_model, _sourceProvider,
				_destinationProvider, _integrationPointType, _OBJECT_TYPE_GUID).Returns(new ValidationResult());

			_permissionValidatorMock.ValidateStop(_model, _sourceProvider,
				_destinationProvider, _integrationPointType, _OBJECT_TYPE_GUID).Returns(new ValidationResult());

			_providerValidatorMock.Validate(_model, _sourceProvider,
				_destinationProvider, _integrationPointType, _OBJECT_TYPE_GUID).Returns(new ValidationResult());

			_validationContex.UserId = 0;

			// ACT

			var onRunException = Assert.Throws<PermissionException>(() => _subjectUnderTest.ValidateOnRun(_validationContex));

			var onSaveException = Assert.Throws<PermissionException>(() => _subjectUnderTest.ValidateOnSave(_validationContex));

			var onStopException = Assert.Throws<PermissionException>(() => _subjectUnderTest.ValidateOnStop(_validationContex));

			// ASSERT

			Assert.That(onRunException.Message.Contains(Constants.IntegrationPoints.NO_USERID));

			Assert.That(onSaveException.Message.Contains(Constants.IntegrationPoints.NO_USERID));

			Assert.That(onStopException.Message.Contains(Constants.IntegrationPoints.NO_USERID));
		}

		[Test]
		public void ItShouldNotThrowAnyValidationExceptionOnRun()
		{
			// ARRANGE

			_permissionValidatorMock.Validate(_model, _sourceProvider,
				_destinationProvider, _integrationPointType, _OBJECT_TYPE_GUID).Returns(new ValidationResult());

			_providerValidatorMock.Validate(_model, _sourceProvider,
				_destinationProvider, _integrationPointType, _OBJECT_TYPE_GUID).Returns(new ValidationResult());

			// ACT

			_subjectUnderTest.ValidateOnRun(_validationContex);

			// ASSERT

			_permissionValidatorMock.Received(1).Validate(_model, _sourceProvider,
				_destinationProvider, _integrationPointType, _OBJECT_TYPE_GUID);

			_permissionValidatorMock.DidNotReceive().ValidateSave(Arg.Any<IntegrationPointModelBase>(), Arg.Any<SourceProvider>(),
				Arg.Any<DestinationProvider>(), Arg.Any<IntegrationPointType>(), Arg.Any<string>());

			_permissionValidatorMock.DidNotReceive().ValidateStop(Arg.Any<IntegrationPointModelBase>(), Arg.Any<SourceProvider>(),
				Arg.Any<DestinationProvider>(), Arg.Any<IntegrationPointType>(), Arg.Any<string>());

			_providerValidatorMock.Received(1).Validate(_model, _sourceProvider,
				_destinationProvider, _integrationPointType, _OBJECT_TYPE_GUID);
		}

		[Test]
		public void ItShouldReturnPermissionValidationViolationOnRun()
		{
			// ARRANGE
			string expectedMessage = "Permission Issue!";

			_permissionValidatorMock.Validate(_model, _sourceProvider, _destinationProvider, _integrationPointType, _OBJECT_TYPE_GUID)
				.Returns(new ValidationResult(new [] { expectedMessage }));

			_providerValidatorMock.Validate(_model, _sourceProvider,
				_destinationProvider, _integrationPointType, _OBJECT_TYPE_GUID).Returns(new ValidationResult());

			// ACT

			var permissionException = Assert.Throws<PermissionException>(() => _subjectUnderTest.ValidateOnRun(_validationContex));

			// ASSERT

			Assert.That(permissionException.Message.Contains(expectedMessage));

			_permissionValidatorMock.Received(1).Validate(_model, _sourceProvider,
				_destinationProvider, _integrationPointType, _OBJECT_TYPE_GUID);

			_permissionValidatorMock.DidNotReceive().ValidateSave(Arg.Any<IntegrationPointModelBase>(), Arg.Any<SourceProvider>(),
				Arg.Any<DestinationProvider>(), Arg.Any<IntegrationPointType>(), Arg.Any<string>());

			_permissionValidatorMock.DidNotReceive().ValidateStop(Arg.Any<IntegrationPointModelBase>(), Arg.Any<SourceProvider>(),
				Arg.Any<DestinationProvider>(), Arg.Any<IntegrationPointType>(), Arg.Any<string>());

			_providerValidatorMock.DidNotReceive().Validate(_model, _sourceProvider,
				_destinationProvider, _integrationPointType, _OBJECT_TYPE_GUID);
		}

		[Test]
		public void ItShouldReturnProviderValidationViolationOnRun()
		{
			// ARRANGE
			string expectedMessage = "Provider Issue!";

			_permissionValidatorMock.Validate(_model, _sourceProvider, 
					_destinationProvider, _integrationPointType, _OBJECT_TYPE_GUID).Returns(new ValidationResult());

			_providerValidatorMock.Validate(_model, _sourceProvider,
				_destinationProvider, _integrationPointType, _OBJECT_TYPE_GUID).Returns(new ValidationResult(new[] { expectedMessage }));

			// ACT

			var permissionException = Assert.Throws<IntegrationPointProviderValidationException>(() => _subjectUnderTest.ValidateOnRun(_validationContex));

			// ASSERT

			Assert.That(permissionException.Result.Messages.Any(msg => msg.Contains(expectedMessage)));

			// we expect permission will be checked 
			_providerValidatorMock.Received(1).Validate(_model, _sourceProvider,
				_destinationProvider, _integrationPointType, _OBJECT_TYPE_GUID);

			_permissionValidatorMock.Received(1).Validate(_model, _sourceProvider,
				_destinationProvider, _integrationPointType, _OBJECT_TYPE_GUID);

			_permissionValidatorMock.DidNotReceive().ValidateSave(Arg.Any<IntegrationPointModelBase>(), Arg.Any<SourceProvider>(),
				Arg.Any<DestinationProvider>(), Arg.Any<IntegrationPointType>(), Arg.Any<string>());

			_permissionValidatorMock.DidNotReceive().ValidateStop(Arg.Any<IntegrationPointModelBase>(), Arg.Any<SourceProvider>(),
				Arg.Any<DestinationProvider>(), Arg.Any<IntegrationPointType>(), Arg.Any<string>());

		}

		[Test]
		public void ItShouldNotThrowAnyValidationExceptionOnSave()
		{
			// ARRANGE

			_permissionValidatorMock.ValidateSave(_model, _sourceProvider,
				_destinationProvider, _integrationPointType, _OBJECT_TYPE_GUID).Returns(new ValidationResult());

			_providerValidatorMock.Validate(_model, _sourceProvider,
				_destinationProvider, _integrationPointType, _OBJECT_TYPE_GUID).Returns(new ValidationResult());

			// ACT

			_subjectUnderTest.ValidateOnSave(_validationContex);

			// ASSERT

			_permissionValidatorMock.Received(1).ValidateSave(_model, _sourceProvider,
				_destinationProvider, _integrationPointType, _OBJECT_TYPE_GUID);

			_permissionValidatorMock.DidNotReceive().Validate(Arg.Any<IntegrationPointModelBase>(), Arg.Any<SourceProvider>(),
				Arg.Any<DestinationProvider>(), Arg.Any<IntegrationPointType>(), Arg.Any<string>());

			_permissionValidatorMock.DidNotReceive().ValidateStop(Arg.Any<IntegrationPointModelBase>(), Arg.Any<SourceProvider>(),
				Arg.Any<DestinationProvider>(), Arg.Any<IntegrationPointType>(), Arg.Any<string>());

			_providerValidatorMock.Received(1).Validate(_model, _sourceProvider,
				_destinationProvider, _integrationPointType, _OBJECT_TYPE_GUID);
		}

		[Test]
		public void ItShouldReturnPermissionValidationViolationOnSave()
		{
			// ARRANGE
			string expectedMessage = "Permission Issue!";

			_permissionValidatorMock.ValidateSave(_model, _sourceProvider, _destinationProvider, _integrationPointType, _OBJECT_TYPE_GUID)
				.Returns(new ValidationResult(new[] { expectedMessage }));

			_providerValidatorMock.Validate(_model, _sourceProvider,
				_destinationProvider, _integrationPointType, _OBJECT_TYPE_GUID).Returns(new ValidationResult());

			// ACT

			var permissionException = Assert.Throws<PermissionException>(() => _subjectUnderTest.ValidateOnSave(_validationContex));

			// ASSERT

			Assert.That(permissionException.Message.Contains(expectedMessage));

			_permissionValidatorMock.Received(1).ValidateSave(_model, _sourceProvider,
				_destinationProvider, _integrationPointType, _OBJECT_TYPE_GUID);

			_permissionValidatorMock.DidNotReceive().Validate(Arg.Any<IntegrationPointModelBase>(), Arg.Any<SourceProvider>(),
				Arg.Any<DestinationProvider>(), Arg.Any<IntegrationPointType>(), Arg.Any<string>());

			_permissionValidatorMock.DidNotReceive().ValidateStop(Arg.Any<IntegrationPointModelBase>(), Arg.Any<SourceProvider>(),
				Arg.Any<DestinationProvider>(), Arg.Any<IntegrationPointType>(), Arg.Any<string>());

			_providerValidatorMock.DidNotReceive().Validate(_model, _sourceProvider,
				_destinationProvider, _integrationPointType, _OBJECT_TYPE_GUID);
		}

		[Test]
		public void ItShouldReturnProviderValidationViolationOnSave()
		{
			// ARRANGE
			string expectedMessage = "Provider Issue!";

			_permissionValidatorMock.ValidateSave(_model, _sourceProvider,
					_destinationProvider, _integrationPointType, _OBJECT_TYPE_GUID).Returns(new ValidationResult());

			_providerValidatorMock.Validate(_model, _sourceProvider,
				_destinationProvider, _integrationPointType, _OBJECT_TYPE_GUID).Returns(new ValidationResult(new[] { expectedMessage }));

			// ACT

			var providerValidationException = Assert.Throws<IntegrationPointProviderValidationException>(() => _subjectUnderTest.ValidateOnSave(_validationContex));

			// ASSERT

			Assert.That(providerValidationException.Result.Messages.Any(msg => msg.Contains(expectedMessage)));

			// we expect permission will be checked 
			_providerValidatorMock.Received(1).Validate(_model, _sourceProvider,
				_destinationProvider, _integrationPointType, _OBJECT_TYPE_GUID);

			_permissionValidatorMock.Received(1).ValidateSave(_model, _sourceProvider,
				_destinationProvider, _integrationPointType, _OBJECT_TYPE_GUID);

			_permissionValidatorMock.DidNotReceive().Validate(Arg.Any<IntegrationPointModelBase>(), Arg.Any<SourceProvider>(),
				Arg.Any<DestinationProvider>(), Arg.Any<IntegrationPointType>(), Arg.Any<string>());

			_permissionValidatorMock.DidNotReceive().ValidateStop(Arg.Any<IntegrationPointModelBase>(), Arg.Any<SourceProvider>(),
				Arg.Any<DestinationProvider>(), Arg.Any<IntegrationPointType>(), Arg.Any<string>());

		}

		[Test]
		public void ItShouldNotThrowAnyValidationExceptionOnStop()
		{
			// ARRANGE

			_permissionValidatorMock.ValidateStop(_model, _sourceProvider,
				_destinationProvider, _integrationPointType, _OBJECT_TYPE_GUID).Returns(new ValidationResult());

			//_providerValidatorMock.Validate(_model, _sourceProvider,
			//	_destinationProvider, _integrationPointType, _OBJECT_TYPE_GUID).Returns(new ValidationResult());

			// ACT

			_subjectUnderTest.ValidateOnStop(_validationContex);

			// ASSERT

			_permissionValidatorMock.Received(1).ValidateStop(_model, _sourceProvider,
				_destinationProvider, _integrationPointType, _OBJECT_TYPE_GUID);

			_permissionValidatorMock.DidNotReceive().Validate(Arg.Any<IntegrationPointModelBase>(), Arg.Any<SourceProvider>(),
				Arg.Any<DestinationProvider>(), Arg.Any<IntegrationPointType>(), Arg.Any<string>());

			_permissionValidatorMock.DidNotReceive().ValidateSave(Arg.Any<IntegrationPointModelBase>(), Arg.Any<SourceProvider>(),
				Arg.Any<DestinationProvider>(), Arg.Any<IntegrationPointType>(), Arg.Any<string>());

			_providerValidatorMock.DidNotReceive().Validate(Arg.Any<IntegrationPointModelBase>(), Arg.Any<SourceProvider>(),
				Arg.Any<DestinationProvider>(), Arg.Any<IntegrationPointType>(), Arg.Any<string>());
		}

		[Test]
		public void ItShouldReturnPermissionValidationViolationOnStop()
		{
			// ARRANGE
			string expectedMessage = "Permission Issue!";

			_permissionValidatorMock.ValidateStop(_model, _sourceProvider, _destinationProvider, _integrationPointType, _OBJECT_TYPE_GUID)
				.Returns(new ValidationResult(new[] { expectedMessage }));

			_providerValidatorMock.Validate(_model, _sourceProvider,
				_destinationProvider, _integrationPointType, _OBJECT_TYPE_GUID).Returns(new ValidationResult());

			// ACT

			var permissionException = Assert.Throws<PermissionException>(() => _subjectUnderTest.ValidateOnStop(_validationContex));

			// ASSERT

			Assert.That(permissionException.Message.Contains(expectedMessage));

			_permissionValidatorMock.Received(1).ValidateStop(_model, _sourceProvider,
				_destinationProvider, _integrationPointType, _OBJECT_TYPE_GUID);

			_permissionValidatorMock.DidNotReceive().Validate(Arg.Any<IntegrationPointModelBase>(), Arg.Any<SourceProvider>(),
				Arg.Any<DestinationProvider>(), Arg.Any<IntegrationPointType>(), Arg.Any<string>());

			_permissionValidatorMock.DidNotReceive().ValidateSave(Arg.Any<IntegrationPointModelBase>(), Arg.Any<SourceProvider>(),
				Arg.Any<DestinationProvider>(), Arg.Any<IntegrationPointType>(), Arg.Any<string>());

			_providerValidatorMock.DidNotReceive().Validate(Arg.Any<IntegrationPointModelBase>(), Arg.Any<SourceProvider>(),
				Arg.Any<DestinationProvider>(), Arg.Any<IntegrationPointType>(), Arg.Any<string>());
		}

		#endregion //Tests
	}
}