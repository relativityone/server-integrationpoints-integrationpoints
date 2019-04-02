﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Relativity.Sync.Executors.Validation
{
	/// <summary>
	/// Holds single validation result and aggregated messages
	/// </summary>
	internal sealed class ValidationResult
	{
		private const string _MESSAGE_PREFIX = "Integration Point validation failed.";

		private readonly List<ValidationMessage> _messages = new List<ValidationMessage>();

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

			if (!string.IsNullOrWhiteSpace(message))
			{
				_messages.Add(new ValidationMessage(message));
			}
		}

		/// <summary>
		/// Constructor, sets validation result and message
		/// </summary>
		/// <param name="message">Validation message</param>
		public ValidationResult(ValidationMessage message)
		{
			if (message != null)
			{
				Add(message);
			}
		}
		
		/// <summary>
		/// Constructor, sets validation result and message
		/// </summary>
		/// <param name="messages">Validation message</param>
		public ValidationResult(IEnumerable<ValidationMessage> messages)
		{
			AddRange(messages);
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
		public IEnumerable<ValidationMessage> Messages => _messages;

		public IEnumerable<string> MessageTexts => _messages.Select(m => m.ToString());

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
			IsValid = false;
			_messages.Add(validationMessage);
		}

		/// <summary>
		/// Adds validation message to internal collection
		/// </summary>
		/// <param name="shortMessage">Message short text</param>
		/// <remarks>Only non-empty messages will be added and change validation state to false</remarks>
		public void Add(string shortMessage)
		{
			Add(string.Empty, shortMessage);
		}

		/// <summary>
		/// Adds validation message to internal collection
		/// </summary>
		/// <param name="errorCode">Message error code</param>
		/// <param name="shortMessage">Message short text</param>
		/// <remarks>Only non-empty messages will be added and change validation state to false</remarks>
		public void Add(string errorCode, string shortMessage)
		{
			if (string.IsNullOrWhiteSpace(shortMessage))
			{
				return;
			}

			IsValid = false;

			_messages.Add(new ValidationMessage(errorCode, shortMessage));
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

		private void AddRange(IEnumerable<ValidationMessage> messages)
		{
			if (messages != null)
			{
				foreach (var message in messages)
				{
					Add(message);
				}
			}
		}

		public override string ToString()
		{
			IEnumerable<string> messageStrings = _messages.Select(m => m.ToString());
			string resultMessage = string.Join(Environment.NewLine, messageStrings);
			return $"{_MESSAGE_PREFIX}{Environment.NewLine}{resultMessage}";
		}
	}
}