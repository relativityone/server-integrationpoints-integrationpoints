using System;
using System.Collections.Generic;
using System.Linq;

namespace Relativity.Sync.Executors.Validation
{
	/// <summary>
	/// Represents validation result and aggregated messages
	/// </summary>
	[Serializable]
	public sealed class ValidationResult
	{
		private static readonly ValidationResult _invalid = new ValidationResult()
		{
			IsValid = false
		};

		private readonly List<ValidationMessage> _messages = new List<ValidationMessage>();

		/// <summary>
		/// Default constructor which sets validation result as true
		/// </summary>
		public ValidationResult()
		{
		}

		/// <summary>
		/// Constructor, sets validation result and message
		/// </summary>
		/// <param name="messages">Validation messages to add</param>
		public ValidationResult(params ValidationMessage[] messages)
		{
			AddRange(messages);
		}

		/// <summary>
		/// Validation result
		/// </summary>
		public bool IsValid { get; set; } = true;

		/// <summary>
		/// Collection of validation messages
		/// </summary>
		public IEnumerable<ValidationMessage> Messages => _messages;

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
		/// <param name="validationMessage">Message to add</param>
		/// <remarks>Only non-empty messages will be added and change validation state to false</remarks>
		public void Add(ValidationMessage validationMessage)
		{
			if (validationMessage != null)
			{
				IsValid = false;
				_messages.Add(validationMessage);
			}
		}

		/// <summary>
		/// Adds validation message to internal collection
		/// </summary>
		/// <param name="shortMessage">Message short text</param>
		/// <remarks>Only non-empty messages will be added and change validation state to false</remarks>
		public void Add(string shortMessage)
		{
			if (string.IsNullOrWhiteSpace(shortMessage))
			{
				return;
			}
			Add(new ValidationMessage(shortMessage));
		}

		private void AddRange(IEnumerable<ValidationMessage> messages)
		{
			if (messages != null)
			{
				foreach (var message in messages.Where(m => m != null))
				{
					Add(message);
				}
			}
		}

		/// <inheritdoc />
		public override string ToString()
		{
			IEnumerable<string> messageStrings = _messages.Select(m => m.ToString());
			return string.Join(System.Environment.NewLine, messageStrings);
		}

		internal static ValidationResult Invalid => _invalid;
	}
}