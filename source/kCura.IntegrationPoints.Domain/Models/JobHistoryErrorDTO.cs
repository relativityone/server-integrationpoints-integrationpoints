using System;
using System.Collections.Generic;

namespace kCura.IntegrationPoints.Domain.Models
{
	/// <summary>
	/// DTO representation of Job History Error object
	/// </summary>
	public class JobHistoryErrorDTO : BaseDTO
	{
		public const string ArtifactTypeGuid = "17E7912D-4F57-4890-9A37-ABC2B8A37BDB";
		public const string TableName = "JobHistoryError";

		/// <summary>
		/// Error
		/// </summary>
		public string Error { get; set; }

		/// <summary>
		/// Error Status
		/// </summary>
		public Choices.ErrorStatus.Values ErrorStatus { get; set; }

		/// <summary>
		/// Error Type
		/// </summary>
		public Choices.ErrorType.Values ErrorType { get; set; }

		/// <summary>
		/// Job History
		/// </summary>
		public int? JobHistory { get; set; }

		/// <summary>
		/// Source Unique Id
		/// </summary>
		public string SourceUniqueID  { get; set; }

		/// <summary>
		/// Stack Trace
		/// </summary>
		public string StackTrace { get; set; }

		/// <summary>
		/// Time Stamp UTC
		/// </summary>
		public DateTime? TimestampUTC { get; set; }

		/// <summary>
		/// Field GUIDs for Job History Error object
		/// </summary>
		public class FieldGuids
		{
			public const string ArtifactId = @"56626016-FA1C-4A96-84A9-C41E68B3015A";
			public const string Error = @"4112B894-35B0-4E53-AB99-C9036D08269D";
			public const string ErrorStatus = @"DE1A46D2-D615-427A-B9F2-C10769BC2678";
			public const string ErrorType = @"EEFFA5D3-82E3-46F8-9762-B4053D73F973";
			public const string JobHistory = @"8B747B91-0627-4130-8E53-2931FFC4135F";
			public const string Name = @"84E757CC-9DA2-435D-B288-0C21EC589E66";
			public const string SourceUniqueID = @"5519435E-EE82-4820-9546-F1AF46121901";
			public const string StackTrace = @"0353DBDE-9E00-4227-8A8F-4380A8891CFF";
			public const string TimestampUTC = @"B9CBA772-E7C9-493E-B7F8-8D605A6BFE1F";
		}

		/// <summary>
		/// Column names for Job History Error table
		/// </summary>
		public class FieldNames
		{
			public const string Error = "Error";
			public const string ErrorStatus = "ErrorStatus";
			public const string ErrorType = "ErrorType";
			public const string JobHistory = "JobHistory";
			public const string SourceUniqueID = "SourceUniqueID";
			public const string StackTrace = "StackTrace";
			public const string TimestampUTC = "TimestampUTC";

			public static IEnumerable<string> FieldNamesList = new[]
			{
				"ArtifactID",
				Error,
				ErrorStatus,
				ErrorType,
				JobHistory,
				"Name",
				SourceUniqueID,
				StackTrace,
				TimestampUTC
			};
		}

		/// <summary>
		/// Choices for Job History Error object
		/// </summary>
		public static class Choices
		{
			/// <summary>
			/// Choices for Error Status
			/// </summary>
			public static class ErrorStatus
			{
				public enum Values
				{
					Expired,
					InProgress,
					New,
					Retried
				}

				public static class Guids
				{
					public static readonly Guid Expired = new Guid("AF01A8FA-B419-49B1-BD71-25296E221E57");
					public static readonly Guid InProgress = new Guid("E5EBD98C-C976-4FA2-936F-434E265EA0AA");
					public static readonly Guid New = new Guid("F881B199-8A67-4D49-B1C1-F9E68658FB5A");
					public static readonly Guid Retried = new Guid("7D3D393D-384F-434E-9776-F9966550D29A");
				}

				public static IEnumerable<Guid> GuidList = new Guid[]
				{
					Guids.Expired,
					Guids.InProgress,
					Guids.New,
					Guids.Retried
				};

				public static readonly Dictionary<Guid, Values> GuidValues = new Dictionary<Guid, Values>()
				{
					{Guids.Expired, Values.Expired},
					{Guids.InProgress, Values.InProgress},
					{Guids.New, Values.New},
					{Guids.Retried, Values.Retried}
				};
			}

			/// <summary>
			/// Choices for Error Type
			/// </summary>
			public static class ErrorType
			{
				public enum Values
				{
					Item,
					Job
				}
		
				public static class Guids
				{
					public static readonly Guid Item = new Guid("9DDC4914-FEF3-401F-89B7-2967CD76714B");
					public static readonly Guid Job = new Guid("FA8BB625-05E6-4BF7-8573-012146BAF19B");
				}

				public static IEnumerable<Guid> GuidList = new Guid[]
				{
					Guids.Item,
					Guids.Job
				};

				public static readonly Dictionary<Guid, Values> GuidValues = new Dictionary<Guid, Values>()
				{
					{Guids.Item, Values.Item},
					{Guids.Job, Values.Job}
				};
			}
		}

		/// <summary>
		/// Update Type used for which Statuses to Update to
		/// </summary>
		public class UpdateStatusType
		{
			/// <summary>
			/// Job Type
			/// </summary>
			public JobTypeChoices JobType { get; set; }

			/// <summary>
			/// Error Types for Retry
			/// </summary>
			public ErrorTypesChoices ErrorTypes { get; set; }

			public enum JobTypeChoices
			{
				RetryErrors,
				Run
			}

			public enum ErrorTypesChoices
			{
				JobOnly,
				JobAndItem,
				ItemOnly,
				None
			}
		}
	}
}
