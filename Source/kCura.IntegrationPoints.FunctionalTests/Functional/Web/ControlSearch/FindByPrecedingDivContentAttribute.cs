using Relativity.Testing.Framework.Web.ControlSearch;

namespace Relativity.IntegrationPoints.Tests.Functional.Web.ControlSearch
{
	public class FindByPrecedingDivContentAttribute : FindByPrecedingSiblingContentAttribute
	{
		public FindByPrecedingDivContentAttribute()
		{
			SiblingXPath = "div";
			Format = "{0}:";
		}
	}
}
