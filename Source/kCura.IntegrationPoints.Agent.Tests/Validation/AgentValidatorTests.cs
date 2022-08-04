using System;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Agent.Validation;
using kCura.IntegrationPoints.Core.Exceptions;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Data;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Agent.Tests.Validation
{
    [TestFixture, Category("Unit")]
    public class AgentValidatorTests : TestBase
    {
        #region Fields

        private AgentValidator _instanceUnderTest;

        private Data.IntegrationPoint _integrationPoint;

        private ICaseServiceContext _caseContext;
        private IValidationExecutor _validationExecutor;
        

        private const int _ARTIFATC_ID = 1234;
        private const int _TYPE_ID = 1;
        private const int _USER_ID = 2345;
        private const int _SOURCE_PROVIDER_ID = 1;
        private const int _DEST_PROVIDER_ID = 2;


        private readonly DestinationProvider _destinationProvider = new DestinationProvider();

        private readonly SourceProvider _sourceProvider = new SourceProvider();
        private readonly IntegrationPointType _integrationPointType = new IntegrationPointType();

        #endregion // Fields

        #region SetUp

        public override void SetUp()
        {
            _validationExecutor = Substitute.For<IValidationExecutor>();
            _caseContext = Substitute.For<ICaseServiceContext>();

            _integrationPoint = new Data.IntegrationPoint()
            {
                ArtifactId = _ARTIFATC_ID,
                Type = _TYPE_ID,
                SourceProvider = _SOURCE_PROVIDER_ID,
                DestinationProvider = _DEST_PROVIDER_ID,
                Name = "Name",
                OverwriteFields = OverwriteFieldsChoices.IntegrationPointAppendOnly,
                DestinationConfiguration = string.Empty,
                SourceConfiguration = string.Empty,
                EnableScheduler = false,
                ScheduleRule = string.Empty, 
                EmailNotificationRecipients = string.Empty,
                LogErrors= false,
                HasErrors = false,
                LastRuntimeUTC = DateTime.Now,
                NextScheduledRuntimeUTC = DateTime.Now,
                FieldMappings = string.Empty,
                SecuredConfiguration = string.Empty
            };

            _caseContext.RelativityObjectManagerService.RelativityObjectManager.Read<SourceProvider>(_integrationPoint.SourceProvider.Value).Returns(_sourceProvider);
            _caseContext.RelativityObjectManagerService.RelativityObjectManager.Read<DestinationProvider>(_integrationPoint.DestinationProvider.Value).Returns(_destinationProvider);
            _caseContext.RelativityObjectManagerService.RelativityObjectManager.Read<IntegrationPointType>(_integrationPoint.Type.Value).Returns(_integrationPointType);

            _instanceUnderTest = new AgentValidator(_validationExecutor, _caseContext);
        }

        #endregion //SetUp

        [Test]
        public void ItShouldRunValidation()
        {
            // Act

            // Arrange

            _instanceUnderTest.Validate(_integrationPoint, _USER_ID);

            // Assert
            _validationExecutor.Received(1).ValidateOnRun(Arg.Is<ValidationContext>(x =>
                x.IntegrationPointType == _integrationPointType &&
                x.SourceProvider == _sourceProvider &&
                x.UserId == _USER_ID &&
                x.DestinationProvider == _destinationProvider &&
                x.Model.ArtifactID ==  _integrationPoint.ArtifactId &&
                x.ObjectTypeGuid == ObjectTypeGuids.IntegrationPointGuid)
            );

            _caseContext.Received().RelativityObjectManagerService.RelativityObjectManager.Read<SourceProvider>(_integrationPoint.SourceProvider.Value);
            _caseContext.Received().RelativityObjectManagerService.RelativityObjectManager.Read<DestinationProvider>(_integrationPoint.DestinationProvider.Value).Returns(_destinationProvider);
            _caseContext.Received().RelativityObjectManagerService.RelativityObjectManager.Read<IntegrationPointType>(_integrationPoint.Type.Value).Returns(_integrationPointType);
        }

        [Test]
        public void ItShouldThrwoValidationException()
        {
            // Act

            _validationExecutor
                .When(o => o.ValidateOnRun(Arg.Is<ValidationContext>(x =>
                    x.IntegrationPointType == _integrationPointType &&
                    x.SourceProvider == _sourceProvider &&
                    x.UserId == _USER_ID &&
                    x.DestinationProvider == _destinationProvider &&
                    x.Model.ArtifactID == _integrationPoint.ArtifactId &&
                    x.ObjectTypeGuid == ObjectTypeGuids.IntegrationPointGuid)))
                .Do(o => { throw new PermissionException(); });

            // Arrange

            Assert.Throws<PermissionException>(() => _instanceUnderTest.Validate(_integrationPoint, _USER_ID));

            // Assert
            _caseContext.Received().RelativityObjectManagerService.RelativityObjectManager.Read<SourceProvider>(_integrationPoint.SourceProvider.Value);
            _caseContext.Received().RelativityObjectManagerService.RelativityObjectManager.Read<DestinationProvider>(_integrationPoint.DestinationProvider.Value).Returns(_destinationProvider);
            _caseContext.Received().RelativityObjectManagerService.RelativityObjectManager.Read<IntegrationPointType>(_integrationPoint.Type.Value).Returns(_integrationPointType);
        }
    }
}
