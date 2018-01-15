using System;
using kCura.IntegrationPoints.UITests.Pages;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;

namespace kCura.IntegrationPoints.UITests.Auxiliary
{
	public class Grinder
	{
		private const string _URL = @"http://localhost:51067/";

		private const string _SESSION_ID = @"e8d9f5f4915beb8b0e641f8992c4750f";

		public static RemoteWebDriver Driver { get; set; }

		public static void Grind()
		{
			try
			{

			}
			catch (NoSuchFrameException)
			{
				Console.WriteLine(@"Skipping frame switch");
			}

			//PushToRelativitySecondPage second = new PushToRelativitySecondPage(Driver);
			//second.SourceSelect = "Production";
			////second.SourceSelect = "Saved Search";
			//PushToRelativityThirdPage third = second.GoToNextPage();
			//third.MapAllFields();
			//third.SelectCopyNativeFiles("Physical Files");

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
			Console.ReadKey();
		}

	}
}
