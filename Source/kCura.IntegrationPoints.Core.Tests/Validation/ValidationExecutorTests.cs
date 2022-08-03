using System;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Models;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Tests.Validation
{
    [TestFixture, Category("Unit")]
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
        private const int _ZERO_USER_ID = 0;
        private readonly Guid _objectTypeGuid = Guid.NewGuid();


        #endregion // Fields

        #region Setup

        public override void SetUp()
        {
            _providerValidatorMock = Substitute.For<IIntegrationPointProviderValidator>();
            _permissionValidatorMock = Substitute.For<IIntegrationPointPermissionValidator>();

            IHelper helper = Substitute.For<IHelper>();

            _subjectUnderTest = new ValidationExecutor(_providerValidatorMock, _permissionValidatorMock, helper);

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
                ObjectTypeGuid = _objectTypeGuid,
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
                _destinationProvider, _integrationPointType, _objectTypeGuid, _ZERO_USER_ID).Returns(new ValidationResult());

            _permissionValidatorMock.ValidateSave(_model, _sourceProvider,
                _destinationProvider, _integrationPointType, _objectTypeGuid, _ZERO_USER_ID).Returns(new ValidationResult());

            _permissionValidatorMock.ValidateStop(_model, _sourceProvider,
                _destinationProvider, _integrationPointType, _objectTypeGuid, _ZERO_USER_ID).Returns(new ValidationResult());

            _providerValidatorMock.Validate(_model, _sourceProvider,
                _destinationProvider, _integrationPointType, _objectTypeGuid, _ZERO_USER_ID).Returns(new ValidationResult());

            _validationContex.UserId = 0;

            // ACT

            var onRunException = Assert.Throws<IntegrationPointValidationException>(() => _subjectUnderTest.ValidateOnRun(_validationContex));

            var onSaveException = Assert.Throws<IntegrationPointValidationException>(() => _subjectUnderTest.ValidateOnSave(_validationContex));

            var onStopException = Assert.Throws<IntegrationPointValidationException>(() => _subjectUnderTest.ValidateOnStop(_validationContex));

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
                _destinationProvider, _integrationPointType, _objectTypeGuid, _USER_ID).Returns(new ValidationResult());

            _providerValidatorMock.Validate(_model, _sourceProvider,
                _destinationProvider, _integrationPointType, _objectTypeGuid, _USER_ID).Returns(new ValidationResult());

            // ACT

            _subjectUnderTest.ValidateOnRun(_validationContex);

            // ASSERT

            _permissionValidatorMock.Received(1).Validate(_model, _sourceProvider,
                _destinationProvider, _integrationPointType, _objectTypeGuid, _USER_ID);

            _permissionValidatorMock.DidNotReceive().ValidateSave(Arg.Any<IntegrationPointModelBase>(), Arg.Any<SourceProvider>(),
                Arg.Any<DestinationProvider>(), Arg.Any<IntegrationPointType>(), Arg.Any<Guid>(), Arg.Any<int>());

            _permissionValidatorMock.DidNotReceive().ValidateStop(Arg.Any<IntegrationPointModelBase>(), Arg.Any<SourceProvider>(),
                Arg.Any<DestinationProvider>(), Arg.Any<IntegrationPointType>(), Arg.Any<Guid>(), Arg.Any<int>());

            _providerValidatorMock.Received(1).Validate(_model, _sourceProvider,
                _destinationProvider, _integrationPointType, _objectTypeGuid, _USER_ID);
        }

        [Test]
        public void ItShouldReturnPermissionValidationViolationOnRun()
        {
            // ARRANGE
            string expectedMessage = "Permission Issue!";

            _permissionValidatorMock.Validate(_model, _sourceProvider, _destinationProvider, _integrationPointType, _objectTypeGuid, _USER_ID)
                .Returns(new ValidationResult(new[] { expectedMessage }));

            _providerValidatorMock.Validate(_model, _sourceProvider,
                _destinationProvider, _integrationPointType, _objectTypeGuid, _USER_ID).Returns(new ValidationResult());

            // ACT

            var permissionException = Assert.Throws<IntegrationPointValidationException>(() => _subjectUnderTest.ValidateOnRun(_validationContex));

            // ASSERT

            Assert.That(permissionException.Message.Contains(expectedMessage));

            _permissionValidatorMock.Received(1).Validate(_model, _sourceProvider,
                _destinationProvider, _integrationPointType, _objectTypeGuid, _USER_ID);

            _permissionValidatorMock.DidNotReceive().ValidateSave(Arg.Any<IntegrationPointModelBase>(), Arg.Any<SourceProvider>(),
                Arg.Any<DestinationProvider>(), Arg.Any<IntegrationPointType>(), Arg.Any<Guid>(), Arg.Any<int>());

            _permissionValidatorMock.DidNotReceive().ValidateStop(Arg.Any<IntegrationPointModelBase>(), Arg.Any<SourceProvider>(),
                Arg.Any<DestinationProvider>(), Arg.Any<IntegrationPointType>(), Arg.Any<Guid>(), Arg.Any<int>());

            _providerValidatorMock.DidNotReceive().Validate(_model, _sourceProvider,
                _destinationProvider, _integrationPointType, _objectTypeGuid, _USER_ID);
        }

        [Test]
        public void ItShouldReturnProviderValidationViolationOnRun()
        {
            // ARRANGE
            string expectedMessage = "Provider Issue!";

            _permissionValidatorMock.Validate(_model, _sourceProvider,
                    _destinationProvider, _integrationPointType, _objectTypeGuid, _USER_ID).Returns(new ValidationResult());

            _providerValidatorMock.Validate(_model, _sourceProvider,
                _destinationProvider, _integrationPointType, _objectTypeGuid, _USER_ID).Returns(new ValidationResult(new[] { expectedMessage }));

            // ACT

            var permissionException = Assert.Throws<IntegrationPointValidationException>(() => _subjectUnderTest.ValidateOnRun(_validationContex));

            // ASSERT

            Assert.That(permissionException.ValidationResult.MessageTexts.Any(msg => msg.Contains(expectedMessage)));

            // we expect permission will be checked 
            _providerValidatorMock.Received(1).Validate(_model, _sourceProvider,
                _destinationProvider, _integrationPointType, _objectTypeGuid, _USER_ID);

            _permissionValidatorMock.Received(1).Validate(_model, _sourceProvider,
                _destinationProvider, _integrationPointType, _objectTypeGuid, _USER_ID);

            _permissionValidatorMock.DidNotReceive().ValidateSave(Arg.Any<IntegrationPointModelBase>(), Arg.Any<SourceProvider>(),
                Arg.Any<DestinationProvider>(), Arg.Any<IntegrationPointType>(), Arg.Any<Guid>(), Arg.Any<int>());

            _permissionValidatorMock.DidNotReceive().ValidateStop(Arg.Any<IntegrationPointModelBase>(), Arg.Any<SourceProvider>(),
                Arg.Any<DestinationProvider>(), Arg.Any<IntegrationPointType>(), Arg.Any<Guid>(), Arg.Any<int>());

        }

        [Test]
        public void ItShouldNotThrowAnyValidationExceptionOnSave()
        {
            // ARRANGE

            _permissionValidatorMock.ValidateSave(_model, _sourceProvider,
                _destinationProvider, _integrationPointType, _objectTypeGuid, _USER_ID).Returns(new ValidationResult());

            _providerValidatorMock.Validate(_model, _sourceProvider,
                _destinationProvider, _integrationPointType, _objectTypeGuid, _USER_ID).Returns(new ValidationResult());

            // ACT

            _subjectUnderTest.ValidateOnSave(_validationContex);

            // ASSERT

            _permissionValidatorMock.Received(1).ValidateSave(_model, _sourceProvider,
                _destinationProvider, _integrationPointType, _objectTypeGuid, _USER_ID);

            _permissionValidatorMock.DidNotReceive().Validate(Arg.Any<IntegrationPointModelBase>(), Arg.Any<SourceProvider>(),
                Arg.Any<DestinationProvider>(), Arg.Any<IntegrationPointType>(), Arg.Any<Guid>(), Arg.Any<int>());

            _permissionValidatorMock.DidNotReceive().ValidateStop(Arg.Any<IntegrationPointModelBase>(), Arg.Any<SourceProvider>(),
                Arg.Any<DestinationProvider>(), Arg.Any<IntegrationPointType>(), Arg.Any<Guid>(), Arg.Any<int>());

            _providerValidatorMock.Received(1).Validate(_model, _sourceProvider,
                _destinationProvider, _integrationPointType, _objectTypeGuid, _USER_ID);
        }

        [Test]
        public void ItShouldReturnPermissionValidationViolationOnSave()
        {
            // ARRANGE
            string expectedMessage = "Permission Issue!";

            _permissionValidatorMock.ValidateSave(_model, _sourceProvider, _destinationProvider, _integrationPointType, _objectTypeGuid, _USER_ID)
                .Returns(new ValidationResult(new[] { expectedMessage }));

            _providerValidatorMock.Validate(_model, _sourceProvider,
                _destinationProvider, _integrationPointType, _objectTypeGuid, _USER_ID).Returns(new ValidationResult());

            // ACT

            var permissionException = Assert.Throws<IntegrationPointValidationException>(() => _subjectUnderTest.ValidateOnSave(_validationContex));

            // ASSERT

            Assert.That(permissionException.Message.Contains(expectedMessage));

            _permissionValidatorMock.Received(1).ValidateSave(_model, _sourceProvider,
                _destinationProvider, _integrationPointType, _objectTypeGuid, _USER_ID);

            _permissionValidatorMock.DidNotReceive().Validate(Arg.Any<IntegrationPointModelBase>(), Arg.Any<SourceProvider>(),
                Arg.Any<DestinationProvider>(), Arg.Any<IntegrationPointType>(), Arg.Any<Guid>(), Arg.Any<int>());

            _permissionValidatorMock.DidNotReceive().ValidateStop(Arg.Any<IntegrationPointModelBase>(), Arg.Any<SourceProvider>(),
                Arg.Any<DestinationProvider>(), Arg.Any<IntegrationPointType>(), Arg.Any<Guid>(), Arg.Any<int>());

            _providerValidatorMock.DidNotReceive().Validate(_model, _sourceProvider,
                _destinationProvider, _integrationPointType, _objectTypeGuid, _USER_ID);
        }

        [Test]
        public void ItShouldReturnProviderValidationViolationOnSave()
        {
            // ARRANGE
            string expectedMessage = "Provider Issue!";

            _permissionValidatorMock.ValidateSave(_model, _sourceProvider,
                    _destinationProvider, _integrationPointType, _objectTypeGuid, _USER_ID).Returns(new ValidationResult());

            _providerValidatorMock.Validate(_model, _sourceProvider,
                _destinationProvider, _integrationPointType, _objectTypeGuid, _USER_ID).Returns(new ValidationResult(new[] { expectedMessage }));

            // ACT

            var providerValidationException = Assert.Throws<IntegrationPointValidationException>(() => _subjectUnderTest.ValidateOnSave(_validationContex));

            // ASSERT

            Assert.That(providerValidationException.ValidationResult.MessageTexts.Any(msg => msg.Contains(expectedMessage)));

            // we expect permission will be checked 
            _providerValidatorMock.Received(1).Validate(_model, _sourceProvider,
                _destinationProvider, _integrationPointType, _objectTypeGuid, _USER_ID);

            _permissionValidatorMock.Received(1).ValidateSave(_model, _sourceProvider,
                _destinationProvider, _integrationPointType, _objectTypeGuid, _USER_ID);

            _permissionValidatorMock.DidNotReceive().Validate(Arg.Any<IntegrationPointModelBase>(), Arg.Any<SourceProvider>(),
                Arg.Any<DestinationProvider>(), Arg.Any<IntegrationPointType>(), Arg.Any<Guid>(), Arg.Any<int>());

            _permissionValidatorMock.DidNotReceive().ValidateStop(Arg.Any<IntegrationPointModelBase>(), Arg.Any<SourceProvider>(),
                Arg.Any<DestinationProvider>(), Arg.Any<IntegrationPointType>(), Arg.Any<Guid>(), Arg.Any<int>());

        }

        [Test]
        public void ItShouldNotThrowAnyValidationExceptionOnStop()
        {
            // ARRANGE

            _permissionValidatorMock.ValidateStop(_model, _sourceProvider,
                _destinationProvider, _integrationPointType, _objectTypeGuid, _USER_ID).Returns(new ValidationResult());

            //_providerValidatorMock.Validate(_model, _sourceProvider,
            //    _destinationProvider, _integrationPointType, _OBJECT_TYPE_GUID).Returns(new ValidationResult());

            // ACT

            _subjectUnderTest.ValidateOnStop(_validationContex);

            // ASSERT

            _permissionValidatorMock.Received(1).ValidateStop(_model, _sourceProvider,
                _destinationProvider, _integrationPointType, _objectTypeGuid, _USER_ID);

            _permissionValidatorMock.DidNotReceive().Validate(Arg.Any<IntegrationPointModelBase>(), Arg.Any<SourceProvider>(),
                Arg.Any<DestinationProvider>(), Arg.Any<IntegrationPointType>(), Arg.Any<Guid>(), Arg.Any<int>());

            _permissionValidatorMock.DidNotReceive().ValidateSave(Arg.Any<IntegrationPointModelBase>(), Arg.Any<SourceProvider>(),
                Arg.Any<DestinationProvider>(), Arg.Any<IntegrationPointType>(), Arg.Any<Guid>(), Arg.Any<int>());

            _providerValidatorMock.DidNotReceive().Validate(Arg.Any<IntegrationPointModelBase>(), Arg.Any<SourceProvider>(),
                Arg.Any<DestinationProvider>(), Arg.Any<IntegrationPointType>(), Arg.Any<Guid>(), Arg.Any<int>());
        }

        [Test]
        public void ItShouldReturnPermissionValidationViolationOnStop()
        {
            // ARRANGE
            string expectedMessage = "Permission Issue!";

            _permissionValidatorMock.ValidateStop(_model, _sourceProvider, _destinationProvider, _integrationPointType, _objectTypeGuid, _USER_ID)
                .Returns(new ValidationResult(new[] { expectedMessage }));

            _providerValidatorMock.Validate(_model, _sourceProvider,
                _destinationProvider, _integrationPointType, _objectTypeGuid, _USER_ID).Returns(new ValidationResult());

            // ACT

            var permissionException = Assert.Throws<IntegrationPointValidationException>(() => _subjectUnderTest.ValidateOnStop(_validationContex));

            // ASSERT

            Assert.That(permissionException.Message.Contains(expectedMessage));

            _permissionValidatorMock.Received(1).ValidateStop(_model, _sourceProvider,
                _destinationProvider, _integrationPointType, _objectTypeGuid, _USER_ID);

            _permissionValidatorMock.DidNotReceive().Validate(Arg.Any<IntegrationPointModelBase>(), Arg.Any<SourceProvider>(),
                Arg.Any<DestinationProvider>(), Arg.Any<IntegrationPointType>(), Arg.Any<Guid>(), Arg.Any<int>());

            _permissionValidatorMock.DidNotReceive().ValidateSave(Arg.Any<IntegrationPointModelBase>(), Arg.Any<SourceProvider>(),
                Arg.Any<DestinationProvider>(), Arg.Any<IntegrationPointType>(), Arg.Any<Guid>(), Arg.Any<int>());

            _providerValidatorMock.DidNotReceive().Validate(Arg.Any<IntegrationPointModelBase>(), Arg.Any<SourceProvider>(),
                Arg.Any<DestinationProvider>(), Arg.Any<IntegrationPointType>(), Arg.Any<Guid>(), Arg.Any<int>());
        }

        #endregion //Tests
    }
}
