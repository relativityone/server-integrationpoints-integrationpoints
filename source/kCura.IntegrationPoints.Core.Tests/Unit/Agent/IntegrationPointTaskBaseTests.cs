using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Core.Agent;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Domain;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Tests.Unit.Agent
{
	[TestFixture]
	public class IntegrationPointTaskBaseTests
	{
		private TestClass _testInstance;

		protected ICaseServiceContext _caseServiceContext;
		protected readonly IHelper _helper;
		protected IDataProviderFactory _dataProviderFactory;
		protected kCura.Apps.Common.Utils.Serializers.ISerializer _serializer;
		protected IJobHistoryService _jobHistoryService;
		protected JobHistoryErrorService _jobHistoryErrorService;
		protected ISynchronizerFactory _appDomainRdoSynchronizerFactoryFactory;
		protected IJobManager _jobManager;

		[SetUp]
		public void SetUp()
		{
			_testInstance = new TestClass(_caseServiceContext,
				_helper,
				_dataProviderFactory,
				_serializer,
				_appDomainRdoSynchronizerFactoryFactory,
				_jobHistoryService,
				_jobHistoryErrorService,
				_jobManager);
		}

		[TestCase("")]
		[TestCase(" ")]
		[TestCase(";")]
		[TestCase(" ;")]
		[TestCase("; ")]
		[TestCase(" ; ")]
		[TestCase(";;")]
		[TestCase(" ; ; ")]
		public void GetRecipientEmails_NoEmailTests(string testEmail)
		{
			// ARRANGE
			var integrationPoint = new Data.IntegrationPoint
			{
				EmailNotificationRecipients = testEmail,
			};

			_testInstance.SetIntegrationPoint(integrationPoint);

			// ACT
			List<string> resultEmails = _testInstance.GetRecipientEmails();

			// ARRANGE
			Assert.IsNotNull(resultEmails);
			Assert.AreEqual(0, resultEmails.Count);

		}

		[Test]
		public void GetRecipientEmails_OneEmailTest()
		{
			// ARRANGE
			string email1 = "email1@kcura.com";

			var integrationPoint = new Data.IntegrationPoint
			{
				EmailNotificationRecipients = email1,
			};

			_testInstance.SetIntegrationPoint(integrationPoint);

			// ACT
			List<string> resultEmails = _testInstance.GetRecipientEmails();

			// ARRANGE
			Assert.IsNotNull(resultEmails);
			Assert.AreEqual(1, resultEmails.Count);
			Assert.AreEqual(email1, resultEmails[0]);
		}

		[Test]
		public void GetRecipientEmails_TwoEmailTest()
		{
			// ARRANGE
			string email1 = "email1@kcura.com";
			string email2 = "email2@kcura.com";

			var integrationPoint = new Data.IntegrationPoint
			{ 
				EmailNotificationRecipients = $"  {email1} ;   {email2}   ",
			};

			_testInstance.SetIntegrationPoint(integrationPoint);

			// ACT
			List<string> resultEmails = _testInstance.GetRecipientEmails();

			// ARRANGE
			Assert.IsNotNull(resultEmails);
			Assert.AreEqual(2, resultEmails.Count);
			Assert.AreEqual(email1, resultEmails[0]);
			Assert.AreEqual(email2, resultEmails[1]);
		}

		[Test]
		public void GetRecipientEmails_MultipleEmailTest()
		{
			// ARRANGE
			string email1 = "email1@kcura.com";
			string email2 = "email2@kcura.com";
			string email3 = "email3@kcura.com";

			var integrationPoint = new Data.IntegrationPoint
			{
				EmailNotificationRecipients = $"  {email1} ;  ;;;  ;;;;  {email2}   ;  {email3} ;;;",
			};

			_testInstance.SetIntegrationPoint(integrationPoint);

			// ACT
			List<string> resultEmails = _testInstance.GetRecipientEmails();

			// ARRANGE
			Assert.IsNotNull(resultEmails);
			Assert.AreEqual(3, resultEmails.Count);
			Assert.AreEqual(email1, resultEmails[0]);
			Assert.AreEqual(email2, resultEmails[1]);
			Assert.AreEqual(email3, resultEmails[2]);
		}

	}

	public class TestClass : IntegrationPointTaskBase
	{
		public TestClass(ICaseServiceContext caseServiceContext,
			IHelper helper,
			IDataProviderFactory dataProviderFactory,
			kCura.Apps.Common.Utils.Serializers.ISerializer serializer,
			ISynchronizerFactory appDomainRdoSynchronizerFactoryFactory,
			IJobHistoryService jobHistoryService,
			JobHistoryErrorService jobHistoryErrorService,
			IJobManager jobManager)
			: base(caseServiceContext, helper, dataProviderFactory, serializer, appDomainRdoSynchronizerFactoryFactory,
				jobHistoryService, jobHistoryErrorService, jobManager)
		{
		}

		public void SetIntegrationPoint(Data.IntegrationPoint ip)
		{
			base.IntegrationPoint = ip;
		}

		public List<string> GetRecipientEmails()
		{
			return base.GetRecipientEmails();
		}
	}
}
