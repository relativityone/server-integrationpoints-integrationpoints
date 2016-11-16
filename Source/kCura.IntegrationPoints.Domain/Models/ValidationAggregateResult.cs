using System;
using System.Collections.Generic;
using System.Linq;

namespace kCura.IntegrationPoints.Domain.Models
{
	public class ValidationAggregateResult
	{
		private readonly List<string> _messages;

		public ValidationAggregateResult()
		{
			IsValid = true;
			_messages = new List<string>();
		}

		public bool IsValid { get; private set; }

		public IEnumerable<string> Messages
		{
			get
			{
				return _messages;
			}
		}

		public void Add(ValidationResult validationResult)
		{
			if (validationResult == null)
			{
				throw new ArgumentNullException(nameof(validationResult));
			}

			IsValid &= validationResult.IsValid;

			if (!String.IsNullOrWhiteSpace(validationResult.Message))
			{
				_messages.Add(validationResult.Message);
			}
		}
	}
}