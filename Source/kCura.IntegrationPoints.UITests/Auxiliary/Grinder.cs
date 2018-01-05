using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;

namespace kCura.IntegrationPoints.UITests.Auxiliary
{
	public class Grinder
	{
		private const string _URL = @"http://localhost:53803/";

		private const string _SESSION_ID = @"6444b7927280f6314999348c4bb85617";

		public static RemoteWebDriver Driver { get; set; }

		public static void Grind()
		{
			try
			{
				Driver.SwitchTo().Frame("externalPage");
			}
			catch (NoSuchFrameException)
			{
				Console.WriteLine(@"Skipping frame switch");
			}

		}

		public static void Main()
		{
			Console.WriteLine(@"Grind time!");
			Console.WriteLine($@"Executor URL: {_URL}");
			Console.WriteLine($@"SessionId: {_SESSION_ID}");

			Driver = new ReuseRemoteWebDriver(new Uri(_URL), _SESSION_ID);
			Console.WriteLine(@"Driver created");

			Grind();

			Console.WriteLine(@"End of grinding");
		}

	}
}
