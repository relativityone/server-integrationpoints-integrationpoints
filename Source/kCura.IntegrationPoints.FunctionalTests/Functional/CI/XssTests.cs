﻿using Relativity.IntegrationPoints.Tests.Functional.TestsImplementations;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Tests.Functional.CI
{
	[TestType.UI, TestType.System]
	public class XssTests : TestsBase
	{
		private readonly XssTestsImplementation _testsImplementation;

		public XssTests() 
			: base(nameof(XssTests))
		{
			_testsImplementation = new XssTestsImplementation(this);
		}

		[IdentifiedTestCase("70274A8D-0C98-4A02-A659-A1C20D323355", Const.XSS.XSS_JS)]
		public void IntegrationPoint_Name_PreventXssInjection(string integrationPointName)
		{
			_testsImplementation.IntegrationPointNamePreventXssInjection(integrationPointName);
		}

		[IdentifiedTestCase("94C62D85-7F7B-4F7B-8569-D943ECB641E0", Const.XSS.XSS_JS)]
		public void IntegrationPoint_EmailNotificationRecipients_PreventXssInjection(string emailRecipients)
		{
			_testsImplementation.IntegrationPointEmailNotificationRecipientsPreventXssInjection(emailRecipients);
		}

		[IdentifiedTestCase("101140D9-1A7E-487C-A0D9-91AA8405D8B8", Const.XSS.XSS_JS)]
		public void IntegrationPointProfile_Name_PreventXssInjection(string integrationPointName)
		{
			_testsImplementation.IntegrationPointProfileNamePreventXssInjection(integrationPointName);
		}

		[IdentifiedTestCase("26E79F4B-706D-4529-B59B-4B6655DB2AE9", Const.XSS.XSS_JS)]
		public void IntegrationPointProfile_EmailNotificationRecipients_PreventXssInjection(string emailRecipients)
		{
			_testsImplementation.IntegrationPointProfileEmailNotificationRecipientsPreventXssInjection(emailRecipients);
		}

		[IdentifiedTestCase("558CFF97-8D2D-42F1-8C63-1C9876B1CF81", Const.XSS.XSS_JS)]
		public void IntegrationPoint_SaveAsProfile_PreventXssInjection(string profileName)
		{
			_testsImplementation.IntegrationPointSaveAsProfilePreventXssInjection(profileName);
		}
	}
}