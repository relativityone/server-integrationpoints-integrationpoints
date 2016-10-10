using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.Relativity.Client;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Synchronizers.RDO.Tests.Functional
{
	[TestFixture]
	[Ignore("Tesst don't work and need fix")]
	public class ComplexFieldsTests : RelativityProviderTemplate
	{
		public ComplexFieldsTests() : base("ComplexFields - Source", "ComplexFields - Destination")
		{
		}

		[TestCase(FieldType.FixedLengthText)]
		public void CreateFieldsAndVerifyValuesAreTransferredCorrectly(FieldType fieldType)
		{
			//Arrange
			int fieldArtifactId = Fields.CreateField(WorkspaceArtifactId, fieldType);


		}


		[Test]
		[Ignore("incomplete")]
		public void LetsSeeIfWeCanCreateFieldsAndObjects()
		{
			int objectTypeArtifactId = IntegrationPoint.Tests.Core.ObjectType.CreateObjectType(WorkspaceArtifactId, "Kucuk's Little Object");
			Relativity.Client.DTOs.ObjectType objectType = IntegrationPoint.Tests.Core.ObjectType.ReadObjectType(WorkspaceArtifactId, objectTypeArtifactId);

			int dateField = Fields.CreateField(WorkspaceArtifactId, FieldType.Date);
			int decimalField = Fields.CreateField(WorkspaceArtifactId, FieldType.Decimal);
			int wholeNumberField = Fields.CreateField(WorkspaceArtifactId, FieldType.WholeNumber);
			int fixedLengthField = Fields.CreateField(WorkspaceArtifactId, FieldType.FixedLengthText);
			int longTextField = Fields.CreateField(WorkspaceArtifactId, FieldType.LongText);
			int multipleChoiceField = Fields.CreateField(WorkspaceArtifactId, FieldType.MultipleChoice);

			for (int i = 1; i < 6; i++)
			{
				IntegrationPoint.Tests.Core.Choice.CreateChoice(WorkspaceArtifactId, multipleChoiceField, $"RIP - MultipleChoice{i}", (i * 10));
			}

			int singleChoiceField = Fields.CreateField(WorkspaceArtifactId, FieldType.SingleChoice);

			for (int i = 1; i < 4; i++)
			{
				IntegrationPoint.Tests.Core.Choice.CreateChoice(WorkspaceArtifactId, singleChoiceField, $"RIP - SingleChoice{i}", (i * 10));
			}

			int multipleObjectField = Fields.CreateField(WorkspaceArtifactId, FieldType.MultipleObject, objectType);
			int singleObjectField = Fields.CreateField(WorkspaceArtifactId, FieldType.SingleObject, objectType);
			int yesNoField = Fields.CreateField(WorkspaceArtifactId, FieldType.YesNo);
		}
	}
}