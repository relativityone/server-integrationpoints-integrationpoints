using OpenQA.Selenium;

namespace kCura.IntegrationPoints.UITests.Components
{
	public abstract class Component
	{
		public IWebDriver Driver { get; }

		protected readonly IWebElement Parent;

		protected Component(IWebElement parent, IWebDriver driver)
		{
			Driver = driver;
			Parent = parent;
		}
		

	}
}
