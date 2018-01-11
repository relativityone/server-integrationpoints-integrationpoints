using OpenQA.Selenium;

namespace kCura.IntegrationPoints.UITests.Components
{
	public abstract class Component
	{
		protected readonly ISearchContext Parent;

		protected Component(ISearchContext parent)
		{
			Parent = parent;
		}
		
	}
}
