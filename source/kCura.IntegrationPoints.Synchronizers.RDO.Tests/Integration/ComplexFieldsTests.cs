using kCura.Relativity.Client;
using NUnit.Framework;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Templates;

namespace kCura.IntegrationPoints.Synchronizers.RDO.Tests.Integration
{
	[TestFixture]
	[Category("Integration Tests")]
	public class ComplexFieldsTests : RelativityProviderTemplate
	{
		public ComplexFieldsTests() : base("ComplexFields - Source", "ComplexFields - Destination")
		{
		}
		
		[Test]
		[Ignore("incomplete")]
		public void LetsSeeIfWeCanCreateFieldsAndObjects()
		{
			int objectTypeArtifactId = IntegrationPoint.Tests.Core.ObjectType.CreateObjectTypeViaRsapi(WorkspaceArtifactId, "Kucuk's Little Object");
			Relativity.Client.DTOs.ObjectType objectType = IntegrationPoint.Tests.Core.ObjectType.ReadObjectTypeViaRsapi(WorkspaceArtifactId, objectTypeArtifactId);

			int dateField = Fields.CreateFieldViaRsapi(WorkspaceArtifactId, FieldType.Date);
			int decimalField = Fields.CreateFieldViaRsapi(WorkspaceArtifactId, FieldType.Decimal);
			int wholeNumberField = Fields.CreateFieldViaRsapi(WorkspaceArtifactId, FieldType.WholeNumber);
			int fixedLengthField = Fields.CreateFieldViaRsapi(WorkspaceArtifactId, FieldType.FixedLengthText);
			int longTextField = Fields.CreateFieldViaRsapi(WorkspaceArtifactId, FieldType.LongText);
			int multipleChoiceField = Fields.CreateFieldViaRsapi(WorkspaceArtifactId, FieldType.MultipleChoice);

			for (int i = 1; i < 6; i++)
			{
				IntegrationPoint.Tests.Core.Choice.CreateChoiceViaRsapi(WorkspaceArtifactId, multipleChoiceField, $"RIP - MultipleChoice{i}", (i * 10));
			}

			int singleChoiceField = Fields.CreateFieldViaRsapi(WorkspaceArtifactId, FieldType.SingleChoice);

			for (int i = 1; i < 4; i++)
			{
				IntegrationPoint.Tests.Core.Choice.CreateChoiceViaRsapi(WorkspaceArtifactId, singleChoiceField, $"RIP - SingleChoice{i}", (i * 10));
			}

			int multipleObjectField = Fields.CreateFieldViaRsapi(WorkspaceArtifactId, FieldType.MultipleObject, objectType);
			int singleObjectField = Fields.CreateFieldViaRsapi(WorkspaceArtifactId, FieldType.SingleObject, objectType);
			int yesNoField = Fields.CreateFieldViaRsapi(WorkspaceArtifactId, FieldType.YesNo);
		}
	}
}
