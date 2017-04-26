using kCura.Relativity.Client;
using Field = kCura.Relativity.Client.DTOs.Field;

namespace kCura.IntegrationPoints.Data.Extensions
{
	public static class FieldExtensions
	{
		public static string GetFieldTypeName(this Field field)
		{
			switch (field.FieldTypeID)
			{
				case FieldType.Currency:
					return FieldTypes.Currency;
				case FieldType.Date:
					return FieldTypes.Date;
				case FieldType.Decimal:
					return FieldTypes.Decimal;
				case FieldType.File:
					return FieldTypes.File;
				case FieldType.FixedLengthText:
					return FieldTypes.FixedLengthText;
				case FieldType.LongText:
					return FieldTypes.LongText;
				case FieldType.MultipleChoice:
					return FieldTypes.MultipleChoice;
				case FieldType.MultipleObject:
					return FieldTypes.MultipleObject;
				case FieldType.SingleChoice:
					return FieldTypes.SingleChoice;
				case FieldType.SingleObject:
					return FieldTypes.SingleObject;
				case FieldType.User:
					return FieldTypes.User;
				case FieldType.WholeNumber:
					return FieldTypes.WholeNumber;
				case FieldType.YesNo:
					return FieldTypes.YesNo;
				default:
					return string.Empty;
			}
		}
	}
}