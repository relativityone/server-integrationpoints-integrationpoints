using Relativity.Testing.Framework.Web.Models;

namespace Relativity.IntegrationPoints.Tests.Functional.Web.Models
{
	internal class RelativityProviderMapFieldsWithImages
    {
		public RelativityProviderOverwrite Overwrite { get; set; }
		public RelativityProviderMultiSelectField FieldOverlayBehavior { get; set; }
        public YesNo CopyImages { get; set; }
		public RelativityProviderImagePrecedence ImagePrecedence { get; set; }
        public YesNo CopyFilesToRepository { get; set; }
	}
}
