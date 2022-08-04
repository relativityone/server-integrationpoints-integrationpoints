using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;

namespace kCura.IntegrationPoints.Domain.Models
{
    /// <summary>
    /// This class holds single validation result and aggregated messages
    /// </summary>
    public class ValidationResult
    {
        private const string _MESSAGE_PREFIX_FAILED = "Integration Point validation failed.";
        private const string _MESSAGE_PREFIX_PASSED = "Integration Point validation passed.";

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

        /// <summary>
        /// Appends validation message to internal existing message
        /// </summary>
        /// <param name="searchShortMessage">Message where text will be appended</param>
        /// <param name="appendShortMessage">Text to append into validation message</param>
        public void AppendTextToShortMessage(string searchShortMessage, string appendShortMessage)
        {
            int index = _messages.FindIndex(x => x.ShortMessage.Contains(searchShortMessage));

            if (index != -1)
            {
                string errorCode = _messages[index].ErrorCode;
                string shortMessage = _messages[index].ShortMessage;

                _messages.RemoveAt(index);
                _messages.Add(new ValidationMessage(errorCode, shortMessage + appendShortMessage));
            }
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
            string prefix = IsValid ? _MESSAGE_PREFIX_PASSED : _MESSAGE_PREFIX_FAILED;
            return $"{prefix}{Environment.NewLine}{resultMessage}";
        }
    }
}