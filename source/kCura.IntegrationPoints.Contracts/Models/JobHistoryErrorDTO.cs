using System;
using System.Collections.Generic;

namespace kCura.IntegrationPoints.Contracts.Models
{
	/// <summary>
	/// DTO representation of Job History Error object
	/// </summary>
	/// <remarks>This is not a complete DTO. As you work on other field values, please add them.</remarks>
	public class JobHistoryErrorDTO : BaseDTO
	{
		/// <summary>
		/// Error Status
		/// </summary>
		public Choices.ErrorStatus.Values ErrorStatus { get; set; }

		/// <summary>
		/// Job History
		/// </summary>
		public int[] JobHistory { get; set; }

		/// <summary>
		/// Field GUIDs for Job History Error object
		/// </summary>
		public class FieldGuids
		{
			public const string ErrorStatus = @"DE1A46D2-D615-427A-B9F2-C10769BC2678";
			public const string JobHistory = @"8B747B91-0627-4130-8E53-2931FFC4135F";
		}

		/// <summary>
		/// Choices for Job History Error object
		/// </summary>
		public static class Choices
		{
			public static class ErrorStatus
			{
				public enum Values
				{
					New,
					Expired,
					InProgress,
					Retried
				}

				public static readonly Dictionary<Guid, Values> GuidValues = new Dictionary<Guid, Values>()
				{
					{new Guid("F881B199-8A67-4D49-B1C1-F9E68658FB5A"), Values.New},
					{new Guid("AF01A8FA-B419-49B1-BD71-25296E221E57"), Values.Expired},
					{new Guid("E5EBD98C-C976-4FA2-936F-434E265EA0AA"), Values.InProgress},
					{new Guid("E5EBD98C-C976-4FA2-936F-434E265EA0AA"), Values.Retried},
				};
			}
		}
	}
}
