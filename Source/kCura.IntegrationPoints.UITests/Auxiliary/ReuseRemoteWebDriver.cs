using System;
using System.Collections.Generic;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;

namespace kCura.IntegrationPoints.UITests.Auxiliary
{
	public class ReuseRemoteWebDriver : RemoteWebDriver
	{
		private readonly string _sessionId;

		public ReuseRemoteWebDriver(Uri remoteAddress, string sessionId) : base(remoteAddress, new ChromeOptions())
		{
			_sessionId = sessionId;
			System.Reflection.FieldInfo sessionIdBase = GetType()
				.BaseType
				.GetField("sessionId",
					System.Reflection.BindingFlags.Instance |
					System.Reflection.BindingFlags.NonPublic);
			sessionIdBase.SetValue(this, new SessionId(sessionId));
		}

		protected override Response Execute(string driverCommandToExecute, Dictionary<string, object> parameters)
		{
			if (driverCommandToExecute == DriverCommand.NewSession)
			{
				var resp = new Response
				{
					Status = WebDriverResult.Success,
					SessionId = _sessionId,
					Value = new Dictionary<string, object>()
				};
				return resp;
			}
			Response respBase = base.Execute(driverCommandToExecute, parameters);
			return respBase;
		}
	}
}