using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using kCura.IntegrationPoints.Core.Validation.Implementation;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;
using NSubstitute;

namespace kCura.IntegrationPoints.Core.Tests.Validation
{

	[TestFixture]
	public class EmailValidatorTest
	{
		private IValidator _instance;

		[SetUp]
		public void Setup()
		{
			_instance = Substitute.For<EmailValidator>();
		}

		[Test]
		[TestCase(null)]
		[TestCase("")]
		[TestCase(";")]
		[TestCase(";;")]
		[TestCase("test@domain.com")]
		[TestCase("test@domain.com;")]
		[TestCase(";test@domain.com")]
		[TestCase("t$est@domain.com")]
		[TestCase("t&est@domain.com")]
		[TestCase("t*est@domain.com")]
		[TestCase("t+est@domain.com")]
		[TestCase("t=est@domain.com")]
		[TestCase("t?est@domain.com")]
		[TestCase("t!est@domain.com")]
		[TestCase("t#est@domain.com")]
		[TestCase("david.jones@proseware.com")]
		[TestCase("d.j@server1.proseware.com")]
		[TestCase("jones@ms1.proseware.com")]
		[TestCase("j@proseware.com9")]
		[TestCase("js#internal@proseware.com")]
		[TestCase("j_9@[129.126.118.1]")]
		[TestCase("js@proseware.com9")]
		[TestCase("j.s@server1.proseware.com")]
		[TestCase("\"j\\\"s\\\"\"@proseware.com")]
		[TestCase("js @contoso.中国")]
		[TestCase("test@domain.com;			")]
		[TestCase("test@domain.com	;			")]
		[TestCase("test@domain.com		;	 test2@domain.com")]
		[TestCase("test@domain.com		;\ntest2@domain.com")]
		[TestCase("test@domain.com; test2@domain.com")]
		[TestCase("test1234@domain.com;2134@domain.com")]
		public void Validate_Valid_Notification_Emails_List(string emails)
		{
			//Arrange

			//Act
			ValidationResult result = _instance.Validate(emails);

			//Assert
			Assert.IsTrue(result.IsValid);
			Assert.IsNull(result.Message);
		}

		[Test]
		[TestCase("test@domain")]
		[TestCase("test@domain..com")]
		[TestCase("test@domain@com")]
		[TestCase("test@domain.")]
		[TestCase("testdomain.com")]
		[TestCase("testdomaincom")]
		[TestCase("t\\est@domain.com")]
		[TestCase("t[est@domain.com")]
		[TestCase("t]est@domain.com")]
		[TestCase("j.@server1.proseware.com")]
		[TestCase("j..s@proseware.com")]
		[TestCase("js*@proseware.com")]
		[TestCase("js@proseware..com")]
		public void Validate_Invalid_Emails(string emails)    //TODO rename
		{
			//Arrange

			//Act
			ValidationResult result = _instance.Validate(emails);

			//Assert
			Assert.IsFalse(result.IsValid);
		}
		
		[Test]
		[TestCase("test@domain.com; ;test2@domain.com", new[] { true, false, true })]
		[TestCase("test@domain.com;#$%@domain.com;test2@domain.com", new [] {true, false, true})]
		[TestCase("test@domain.com;#$%@domain.com;test2@domain.com;", new [] {true, false, true})]
		[TestCase("test@domain.com;testdomain.com ; test@domain..com;	test2@domain.com;", new [] {true, false, false, true})]
		public void Validate_Notification_Emails_List(string emails, bool[] isValidList)
		{
			//Arrange
			const string errorMessage = "Invalid e-mails: ";
			string[] emailList =
				emails.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries)
					.Select(x => x.Trim())
					.ToArray();
			List<string> invalidEmailList = new List<string>();
			for (int i = 0; i < isValidList.Length ; i++)
			{
				if(!isValidList[i]) { invalidEmailList.Add(emailList[i]);}
			}

			//Act
			ValidationResult result = _instance.Validate(emails);

			//Assert
			Assert.IsFalse(result.IsValid);
			List<string> messageEmails = result.Message.Replace(errorMessage, "")
				.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries)
				.ToList();
			foreach (string invalidEmail in invalidEmailList)
			{
				Assert.That(messageEmails.Contains(invalidEmail), "ValidationResult message does not contains invalid email: " + invalidEmail);
			}
		}
	}
}
