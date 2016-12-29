using System;
using System.Collections.Generic;
using System.Linq;

namespace kCura.IntegrationPoints.Domain.Models
{
	/// <summary>
	/// This class holds single validation result and aggregated messages
	/// </summary>
	public class ValidationResult
	{
		private readonly List<string> _messages = new List<string>();

		/// <summary>
		/// Default constructor which sets validation result as true
		/// </summary>
		public ValidationResult()
		{
			IsValid = true;
		}

		/// <summary>
		/// Constructor, set validation result
		/// </summary>
		/// <param name="result">Validation result</param>
		public ValidationResult(bool result)
		{
			IsValid = result;
		}

		/// <summary>
		/// Constructor, sets validation result and message
		/// </summary>
		/// <param name="result">Validation result</param>
		/// <param name="message">Validation message</param>
		public ValidationResult(bool result, string message)
		{
			IsValid = result;

			if (!String.IsNullOrWhiteSpace(message))
			{
				_messages.Add(message);
			}
		}

		/// <summary>
		/// Constructor, sets validation messages
		/// </summary>
		/// <param name="messages">Validation messages</param>
		public ValidationResult(IEnumerable<string> messages)
		{
			AddRange(messages);
		}

		/// <summary>
		/// Validation result
		/// </summary>
		public bool IsValid { get; set; }

		/// <summary>
		/// Collection of validation messages
		/// </summary>
		public IEnumerable<string> Messages => _messages;

		/// <summary>
		/// Adds validation result to itself and aggregates non-empty messages
		/// </summary>
		/// <param name="validationResult">Validation result to add</param>
		/// <remarks>This method does not validate the parameter - 'null' will be ignored</remarks>
		public void Add(ValidationResult validationResult)
		{
			if (validationResult == null)
			{
				return;
			}

			IsValid &= validationResult.IsValid;

			AddRange(validationResult.Messages);
		}

		/// <summary>
		/// Adds validation message to internal collection
		/// </summary>
		/// <param name="message">Message to add</param>
		/// <remarks>Only non-empty messages will be added and change validation state to false</remarks>
		public void Add(string message)
		{
			if (String.IsNullOrWhiteSpace(message))
			{
				return;
			}

			IsValid = false;

			_messages.Add(message);
		}

		private void AddRange(IEnumerable<string> messages)
		{
			if (messages != null)
			{
				foreach (var message in messages)
				{
					Add(message);
				}
			}			
		}
	}
}