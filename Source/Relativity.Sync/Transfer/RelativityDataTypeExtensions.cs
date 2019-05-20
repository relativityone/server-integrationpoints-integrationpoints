using System.Collections.Generic;

namespace Relativity.Sync.Transfer
{
	internal static class RelativityDataTypeExtensions
	{
		private static readonly Dictionary<RelativityDataType, string> _relativityDataTypeDisplayNames = new Dictionary<RelativityDataType, string>
		{
			{RelativityDataType.FixedLengthText, "Fixed-Length Text"},
			{RelativityDataType.WholeNumber, "Whole Number"},
			{RelativityDataType.Date, "Date"},
			{RelativityDataType.YesNo, "Yes/No"},
			{RelativityDataType.LongText, "Long Text"},
			{RelativityDataType.SingleChoice, "Single Choice"},
			{RelativityDataType.Decimal, "Decimal"},
			{RelativityDataType.Currency, "Currency"},
			{RelativityDataType.MultipleChoice, "Multiple Choice"},
			{RelativityDataType.File, "File"},
			{RelativityDataType.SingleObject, "Single Object"},
			{RelativityDataType.User, "User"},
			{RelativityDataType.MultipleObject, "Multiple Object"}
		};

		private static readonly Dictionary<string, RelativityDataType> _relativityDataTypeByDisplayName = new Dictionary<string, RelativityDataType>
		{
			{"Fixed-Length Text", RelativityDataType.FixedLengthText},
			{"Whole Number", RelativityDataType.WholeNumber},
			{"Date", RelativityDataType.Date},
			{"Yes/No", RelativityDataType.YesNo},
			{"Long Text", RelativityDataType.LongText},
			{"Single Choice", RelativityDataType.SingleChoice},
			{"Decimal", RelativityDataType.Decimal},
			{"Currency", RelativityDataType.Currency},
			{"Multiple Choice", RelativityDataType.MultipleChoice},
			{"File", RelativityDataType.File},
			{"Single Object", RelativityDataType.SingleObject},
			{"User", RelativityDataType.User},
			{"Multiple Object", RelativityDataType.MultipleObject}
		};

		public static string ToRelativityTypeDisplayName(this RelativityDataType relativityDataType)
		{
			return _relativityDataTypeDisplayNames[relativityDataType];
		}

		public static RelativityDataType ToRelativityDataType(this string relativityTypeDisplayName)
		{
			return _relativityDataTypeByDisplayName[relativityTypeDisplayName];
		}
	}
}