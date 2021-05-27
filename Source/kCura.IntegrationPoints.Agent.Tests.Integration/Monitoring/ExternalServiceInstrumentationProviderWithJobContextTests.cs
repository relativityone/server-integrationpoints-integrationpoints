using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Agent.Context;
using kCura.IntegrationPoints.Agent.Monitoring;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation.Model;
using kCura.IntegrationPoints.Common.Monitoring.Messages;
using kCura.IntegrationPoints.Common.Monitoring.Messages.JobLifetime;
using kCura.IntegrationPoints.Config;
using kCura.IntegrationPoints.Core.Monitoring;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.DataTransfer.MessageService;
using Relativity.DataTransfer.MessageService.MetricsManager.APM;
using System;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core.TestCategories.Attributes;
using Relativity.Testing.Identification;
using kCura.IntegrationPoints.Common.Metrics;

namespace kCura.IntegrationPoints.Agent.Tests.Integration.Monitoring
{
	[SmokeTest]
	[TestFixture]
	[Feature.DataTransfer.IntegrationPoints]
	public class ExternalServiceInstrumentationProviderWithJobContextTests
	{
		private Mock<IConfig> _configMock;
		private Mock<IMetricsManager> _metricManagerMock;
		private Mock<IDateTimeHelper> _dateTimeHelperMock;
		private Mock<IRipMetrics> _ripMetricsMock; 
		private IJobContextProvider _jobContextProvider;
		private IntegrationPointsMessageService _messageService;
		private ExternalServiceInstrumentationProviderWithJobContext _sut;

		private const string _BUCKET_EXTERNALL_CALL = "IntegrationPoints.Performance.ExternalCall";
		private const string _BUCKET_EXTERNALL_CALL_JOB_SUMMARY = "IntegrationPoints.Performance.ExternalCall.Summary";
		private const int _JOB_ID = 232;
		private const int _WORKSPACE_ID = 4342;
		private const string _SERVICE_TYPE = "RSAPI";
		private const string _SERVICE_NAME = "ObjectManager";
		private const string _OPERATION_NAME = "Query";
		private const int _DELAY_FOR_PROCESSING_MESSAGE_IN_MS = 100;
		private static readonly Guid _batchInstance = Guid.NewGuid();

		[SetUp]
		public void SetUp()
		{
			var logger = new Mock<IAPILog>
			{
				DefaultValue = DefaultValue.Mock
			};

			_configMock = new Mock<IConfig>();

			_metricManagerMock = new Mock<IMetricsManager>();
			var metricManagerFactory = new Mock<IMetricsManagerFactory>();
			metricManagerFactory.Setup(x => x.CreateAPMManager()).Returns(_metricManagerMock.Object);
			_dateTimeHelperMock = new Mock<IDateTimeHelper>();
			_ripMetricsMock = new Mock<IRipMetrics>();
			_messageService = new IntegrationPointsMessageService(metricManagerFactory.Object, _configMock.Object, logger.Object, _dateTimeHelperMock.Object, _ripMetricsMock.Object);

			_jobContextProvider = new JobContextProvider();

			_sut = new ExternalServiceInstrumentationProviderWithJobContext(_jobContextProvider, _messageService, logger.Object, new JSONSerializer(), _configMock.Object);
		}

		[IdentifiedTest("483fcd51-1815-4c3a-b4eb-7f892950a308")]
		public async Task ItShouldSendMetricToApm_WhenItIsEnabled_SuccessfulMessage()
		{
			// arrange
			SetupMeasureOfExternallCallConfigValue(true);
			SetupSendLiveApmMetricsConfigValue(true);

			using (_jobContextProvider.StartJobContext(GetJob()))
			{
				IExternalServiceInstrumentation instrumentation = _sut.Create(_SERVICE_TYPE, _SERVICE_NAME, _OPERATION_NAME);
				IExternalServiceInstrumentationStarted startedInstrumentation = instrumentation.Started();

				// act
				startedInstrumentation.Completed();
			}

			// assert
			await Task.Delay(_DELAY_FOR_PROCESSING_MESSAGE_IN_MS).ConfigureAwait(false);
			_metricManagerMock.Verify(x => x.LogCount(
				_BUCKET_EXTERNALL_CALL,
				It.IsAny<long>(),
				It.Is<IMetricMetadata>(metadata => VerifyMetadata(metadata))
			));
		}

		[IdentifiedTest("5c8d8691-9749-46c8-95ee-b19a9b1c419b")]
		public async Task ItShouldSendMetricToApm_WhenItIsEnabled_FailedMessage()
		{
			// arrange
			string errorMsg = "error message";

			SetupMeasureOfExternallCallConfigValue(true);
			SetupSendLiveApmMetricsConfigValue(true);

			using (_jobContextProvider.StartJobContext(GetJob()))
			{
				IExternalServiceInstrumentation instrumentation = _sut.Create(_SERVICE_TYPE, _SERVICE_NAME, _OPERATION_NAME);
				IExternalServiceInstrumentationStarted startedInstrumentation = instrumentation.Started();
				var exception = new ArgumentNullException(errorMsg);

				// act
				startedInstrumentation.Failed(exception);
			}

			// assert
			await Task.Delay(_DELAY_FOR_PROCESSING_MESSAGE_IN_MS).ConfigureAwait(false);
			_metricManagerMock.Verify(x => x.LogCount(
				_BUCKET_EXTERNALL_CALL,
				It.IsAny<long>(),
				It.Is<IMetricMetadata>(metadata => VerifyMetadata(metadata, nameof(ArgumentNullException)))
			));
		}

		[IdentifiedTestCase("4b7fcc29-4224-4022-b798-0587e7e47a0c", false, false)]
		[IdentifiedTestCase("395c1936-f0e6-4d8f-9388-6837adb8720a", false, true)]
		[IdentifiedTestCase("0049b0c5-b698-45a7-ba50-5a1e90ded116", true, false)]
		public async Task ItShouldNotSendMetricToApm_WhenItIsDisabled(bool measureOfExternalCall, bool sendLiveApmMetrics)
		{
			// arrange
			SetupMeasureOfExternallCallConfigValue(measureOfExternalCall);
			SetupSendLiveApmMetricsConfigValue(sendLiveApmMetrics);

			using (_jobContextProvider.StartJobContext(GetJob()))
			{
				IExternalServiceInstrumentation instrumentation = _sut.Create(_SERVICE_TYPE, _SERVICE_NAME, _OPERATION_NAME);
				IExternalServiceInstrumentationStarted startedInstrumentation = instrumentation.Started();

				// act
				startedInstrumentation.Completed();
			}

			// assert
			await Task.Delay(_DELAY_FOR_PROCESSING_MESSAGE_IN_MS).ConfigureAwait(false);
			_metricManagerMock.Verify(x => x.LogCount(_BUCKET_EXTERNALL_CALL, It.IsAny<long>(), It.IsAny<IMetricMetadata>()), Times.Never);
		}

		[IdentifiedTest("52982f20-9946-4cb0-9c96-c5e9fe547ce0")]
		public async Task ItShouldSendAggregatedMetricsWhenJobCompletes()
		{
			// arrange
			SetupMeasureOfExternallCallConfigValue(true);
			SetupSendLiveApmMetricsConfigValue(true);

			var jobStartedMessage = new JobStartedMessage
			{
				CorrelationID = _batchInstance.ToString(),
				JobID = _JOB_ID.ToString(),
				WorkspaceID = _WORKSPACE_ID
			};
			await _messageService.Send(jobStartedMessage).ConfigureAwait(false);
			await Task.Delay(_DELAY_FOR_PROCESSING_MESSAGE_IN_MS).ConfigureAwait(false);

			using (_jobContextProvider.StartJobContext(GetJob()))
			{
				IExternalServiceInstrumentation instrumentation = _sut.Create(_SERVICE_TYPE, _SERVICE_NAME, _OPERATION_NAME);
				IExternalServiceInstrumentationStarted startedInstrumentation = instrumentation.Started();
				startedInstrumentation.Completed();

				IExternalServiceSimpleInstrumentation simpleInstrumentation =
					_sut.CreateSimple(_SERVICE_TYPE, _SERVICE_NAME, _OPERATION_NAME);
				simpleInstrumentation.Execute(() => { });
			}

			var jobCompletedMessage = new JobCompletedMessage
			{
				CorrelationID = _batchInstance.ToString(),
				JobID = _JOB_ID.ToString(),
				WorkspaceID = _WORKSPACE_ID
			};
			await Task.Delay(_DELAY_FOR_PROCESSING_MESSAGE_IN_MS).ConfigureAwait(false);

			// act
			await _messageService.Send(jobCompletedMessage).ConfigureAwait(false);

			// assert
			await Task.Delay(_DELAY_FOR_PROCESSING_MESSAGE_IN_MS).ConfigureAwait(false);
			_metricManagerMock.Verify(x => x.LogCount(
				_BUCKET_EXTERNALL_CALL_JOB_SUMMARY,
				It.IsAny<long>(),
				It.Is<IMetricMetadata>(metadata => VerifySummaryMetadata(metadata, 2))
			));
		}

		private static Job GetJob()
		{
			var taskParameters = new TaskParameters { BatchInstance = _batchInstance };
			return new JobBuilder()
				.WithJobId(_JOB_ID)
				.WithWorkspaceId(_WORKSPACE_ID)
				.WithJobDetails(taskParameters)
				.Build();
		}

		private void SetupMeasureOfExternallCallConfigValue(bool value)
		{
			_configMock.Setup(x => x.MeasureDurationOfExternalCalls).Returns(value);
		}

		private void SetupSendLiveApmMetricsConfigValue(bool value)
		{
			_configMock.Setup(x => x.SendLiveApmMetrics).Returns(value);
		}

		private static bool VerifyMetadata(IMetricMetadata metadata)
		{
			return VerifyMetadata(metadata, failureReason: "");
		}

		private static bool VerifyMetadata(IMetricMetadata metadata, string failureReason)
		{
			bool isValid = true;
			isValid &= metadata.WorkspaceID == _WORKSPACE_ID;
			isValid &= metadata.CorrelationID == _batchInstance.ToString();
			isValid &= (string)metadata.CustomData[nameof(JobMessageBase.JobID)] == _JOB_ID.ToString();
			isValid &= (string)metadata.CustomData[nameof(ExternalCallCompletedMessage.ServiceType)] == _SERVICE_TYPE;
			isValid &= (string)metadata.CustomData[nameof(ExternalCallCompletedMessage.ServiceName)] == _SERVICE_NAME;
			isValid &= (string)metadata.CustomData[nameof(ExternalCallCompletedMessage.OperationName)] == _OPERATION_NAME;
			bool hasFailed = !string.IsNullOrEmpty(failureReason);
			isValid &= (bool)metadata.CustomData[nameof(ExternalCallCompletedMessage.HasFailed)] == hasFailed;
			if (hasFailed)
			{
				isValid &= (string)metadata.CustomData[nameof(ExternalCallCompletedMessage.FailureReason)] == failureReason;
			}
			return isValid;
		}

		private static bool VerifySummaryMetadata(IMetricMetadata metadata, int totalCount)
		{
			bool isValid = true;
			isValid &= metadata.WorkspaceID == _WORKSPACE_ID;
			isValid &= metadata.CorrelationID == _batchInstance.ToString();
			isValid &= (string)metadata.CustomData[nameof(JobMessageBase.JobID)] == _JOB_ID.ToString();
			isValid &= (string)metadata.CustomData["ServiceType"] == _SERVICE_TYPE;
			isValid &= (long)metadata.CustomData["TotalCount"] == totalCount;
			isValid &= (long)metadata.CustomData["FailedCount"] == 0;
			return isValid;
		}
	}
}
