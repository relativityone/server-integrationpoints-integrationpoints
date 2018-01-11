using OpenQA.Selenium;

namespace kCura.IntegrationPoints.UITests.Components
{
	public abstract class Component
	{
		protected readonly IWebElement Parent;

		protected Component(IWebElement parent)
		{
			Parent = parent;
		}
		
	}
}
