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
		[TestCase("test@dom_ain.com")]
		[TestCase("david.jones@proseware.com")]
		[TestCase("d.j@server1.proseware.com")]
		[TestCase("jones@ms1.proseware.com")]
		[TestCase("j@proseware.com9")]
		[TestCase("js#internal@proseware.com")]
		[TestCase("js*@proseware.com")]
		[TestCase("js@proseware.com9")]
		[TestCase("j.s@server1.proseware.com")]
		[TestCase("\"j\\\"s\\\"\"@proseware.com")]
		[TestCase("test@domain.com		;	 test2@domain.com")]
		[TestCase("test@domain.com		;\ntest2@domain.com")]
		[TestCase("test@domain.com; test2@domain.com")]
		[TestCase("test1234@domain.com;2134@domain.com")]
		[TestCase("test@domain.com;\ntest2@domain.com")]
		public void Validate_Valid_Notification_Emails_List(string emails)
		{
			//Act
			ValidationResult result = _instance.Validate(emails);

			//Assert
			Assert.IsTrue(result.IsValid);
			Assert.IsNull(result.Messages.FirstOrDefault());
		}

		[Test]
		[TestCase("test@domain")]
		[TestCase("test@domain..com")]
		[TestCase("test@")]
		[TestCase("test@domain.")]
		[TestCase("test@domain@com")]
		[TestCase("test@dom*ain.com")]
		[TestCase("test@dom@ain.com")]
		[TestCase("test@dom$ain.com")]
		[TestCase("test@dom!ain.com")]
		[TestCase("test@dom%ain.com")]
		[TestCase("test@dom^ain.com")]
		[TestCase("test@dom&ain.com")]
		[TestCase("testdomain.com")]
		[TestCase("testdomaincom")]
		[TestCase("t\\est@domain.com")]
		[TestCase("t[est@domain.com")]
		[TestCase("t]est@domain.com")]
		[TestCase("j.@server1.proseware.com")]
		[TestCase("j_9@[129.126.118.1]")]
		[TestCase("j..s@proseware.com")]
		[TestCase("js@proseware..com")]
		public void Validate_Invalid_Emails(string emails)
		{
			//Arrange
			string validationMessage = EmailValidator.ERROR_INVALID_EMAIL + emails;

			//Act
			ValidationResult result = _instance.Validate(emails);

			//Assert
			Assert.IsFalse(result.IsValid);
			Assert.That(result.Messages.Contains(validationMessage));
		}

		[Test]
		[TestCase("\n")]
		[TestCase("test@domain.com; ;test2@domain.com")]
		[TestCase("test@domain.com;	\t\t")]
		[TestCase("test@domain.com;	")]
		[TestCase("test@domain.com	;			")]
		public void Validate_Missing_Emails(string emails)
		{
			//Act
			ValidationResult result = _instance.Validate(emails);

			//Assert
			Assert.IsFalse(result.IsValid);
			Assert.That(result.Messages.Contains(EmailValidator.ERROR_MISSING_EMAIL));
		}

		[Test]
		[TestCase("test@domain.com;test@#$%.com;test2@domain.com", new [] {true, false, true})]
		[TestCase("test@#$%.com;test@domain.com;test2@domain.com;", new [] {false, true, true})]
		[TestCase("test@domain.com;testdomain.com ; test@domain..com;	test2@domain.com;", new [] {true, false, false, true})]
		public void Validate_Notification_Emails_List(string emails, bool[] isValidList)
		{
			//Arrange
			string[] emailList =
				emails.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries)
					.ToArray();
			var invalidEmailList = new List<string>();
			for (int i = 0; i < isValidList.Length ; i++)
			{
				if(!isValidList[i]) { invalidEmailList.Add(EmailValidator.ERROR_INVALID_EMAIL + emailList[i].Trim());}
			}

			//Act
			ValidationResult result = _instance.Validate(emails);

			//Assert
			Assert.IsFalse(result.IsValid);
			foreach (string invalidEmail in invalidEmailList)
			{
				Assert.That(result.Messages.Contains(invalidEmail), "ValidationResult message does not contain invalid email: " + invalidEmail);
			}
		}
	}
}
