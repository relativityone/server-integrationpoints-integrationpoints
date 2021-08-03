using Atata;

namespace Relativity.IntegrationPoints.Tests.Functional.Web.Models
{
	internal enum RelativityProviderOverwrite
	{
		AppendOnly,
		OverlayOnly,
		[Term("Append/Overlay")]
		AppendOverlay
	}
}
