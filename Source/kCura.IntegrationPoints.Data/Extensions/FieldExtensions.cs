using System;
using Relativity.Services.Interfaces.Field.Models;

namespace kCura.IntegrationPoints.Data.Extensions
{
    public static class FieldExtensions
    {
        public static string GetFieldTypeName(this BaseFieldRequest field)
        {
            Type fieldType = field.GetType();

            if (fieldType == typeof(CurrencyFieldRequest))
            {
                return FieldTypes.Currency;
            }
            else if (fieldType == typeof(DateFieldRequest))
            {
                return FieldTypes.Date;
            }
            else if (fieldType == typeof(DecimalFieldRequest))
            {
                return FieldTypes.Decimal;
            }
            else if (fieldType == typeof(FileFieldRequest))
            {
                return FieldTypes.File;
            }
            else if (fieldType == typeof(FixedLengthFieldRequest))
            {
                return FieldTypes.FixedLengthText;
            }
            else if (fieldType == typeof(LongTextFieldRequest))
            {
                return FieldTypes.LongText;
            }
            else if (fieldType == typeof(MultipleChoiceFieldRequest))
            {
                return FieldTypes.MultipleChoice;
            }
            else if (fieldType == typeof(MultipleObjectFieldRequest))
            {
                return FieldTypes.MultipleObject;
            }
            else if (fieldType == typeof(SingleChoiceFieldRequest))
            {
                return FieldTypes.SingleChoice;
            }
            else if (fieldType == typeof(SingleObjectFieldRequest))
            {
                return FieldTypes.SingleObject;
            }
            else if (fieldType == typeof(UserFieldRequest))
            {
                return FieldTypes.User;
            }
            else if (fieldType == typeof(WholeNumberFieldRequest))
            {
                return FieldTypes.WholeNumber;
            }
            else if (fieldType == typeof(YesNoFieldRequest))
            {
                return FieldTypes.YesNo;
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
