using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace kCura.IntegrationPoints.Domain.Exceptions
{
	[Serializable]
	public class IntegrationPointsException : Exception
	{
		/// <summary>
		/// The one that can be presented on UI
		/// </summary>
		public string UserMessage { get; set; }

		/// <summary>
		/// Returns any detailed exception specific information in a readable format
		/// </summary>
		public string DetailedMessage { get; set; }

		public IntegrationPointsException()
		{
		}

		public IntegrationPointsException(string message): base (message)
		{
		}

		public IntegrationPointsException(string message, Exception innerException): base (message, innerException)
		{
		}

		protected IntegrationPointsException(SerializationInfo info, StreamingContext context): base (info, context)
		{
			UserMessage = info.GetString(nameof(UserMessage));
			DetailedMessage = info.GetString(nameof(DetailedMessage));
		}

		public override string ToString()
		{
			string detailedMessage = GetDetailedInfo();
			if (detailedMessage != null)
			{
				detailedMessage = $"{detailedMessage}{Environment.NewLine}======================={Environment.NewLine}";
			}
			return $"{detailedMessage}{base.ToString()}";
		}

		/// <summary>
		/// Use this method to provide additional information for the exception. Will be used in ToString()
		/// </summary>
		/// <returns>Detailed exception information in a formatted manner</returns>
		protected virtual string GetDetailedInfo()
		{
			return DetailedMessage;
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue(nameof(DetailedMessage), DetailedMessage);
			info.AddValue(nameof(UserMessage), UserMessage);
			base.GetObjectData(info, context);
		}
	}
}
