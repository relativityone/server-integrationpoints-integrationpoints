using NUnit.Framework;
using Relativity.IntegrationPoints.Tests.Functional.TestsImplementations;
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

		protected override void OnSetUpFixture()
		{
			base.OnSetUpFixture();

			_testsImplementation.OnSetUpFixture();
		}

		[Category("FAILING_TEST")]
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

		[IdentifiedTestCase("B97A4280-9EB5-4008-8AB0-30CAB7C3C27F", Const.XSS.XSS_JS)]
		public void IntegrationPointImportLDAP_PreventXssInjection(string inputText)
		{
			_testsImplementation.IntegrationPointImportFromLDAPPreventXssInjection(inputText);
		}

		[IdentifiedTestCase("B7D6C9BE-0615-4C98-B69A-F6F9CDF19D71", Const.XSS.XSS_JS)]
		public void IntegrationPointExportToLoadFile_PreventXssInjection(string inputText)
		{
			_testsImplementation.IntegrationPointExportToLoadFilePreventXssInjection(inputText);
		}

		[IdentifiedTestCase("B178B15B-95FD-4DDC-91EE-5031D695B04C", Const.XSS.XSS_JS)]
		public void IntegrationPointImportFromFTP_PreventXssInjection(string inputText)
		{
			_testsImplementation.IntegrationPointImportFromFTPPreventXssInjection(inputText);
		}
	}
}
