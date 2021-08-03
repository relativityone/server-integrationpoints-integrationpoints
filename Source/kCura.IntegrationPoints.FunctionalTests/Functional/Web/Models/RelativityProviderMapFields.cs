using Relativity.Testing.Framework.Web.Models;

namespace Relativity.IntegrationPoints.Tests.Functional.Web.Models
{
	internal class RelativityProviderMapFields
	{
		public RelativityProviderOverwrite Overwrite { get; set; }

		public YesNo CopyImages { get; set; }

		public RelativityProviderCopyNativeFiles CopyNativeFiles { get; set; }

		public RelativityProviderFolderPathInformation PathInformation { get; set; }
	}
}
