using Atata;
using Relativity.IntegrationPoints.Tests.Functional.Web.Controls;
using Relativity.Testing.Framework.Web.Components;

namespace Relativity.IntegrationPoints.Tests.Functional.Web.Interfaces
{
	public interface IHasTreeItems<T> where T: WorkspacePage<T>
	{
		UnorderedList<TreeItemControl<T>, T> TreeItems { get; }
	}
}
