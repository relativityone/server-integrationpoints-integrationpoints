using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Relativity.Sync.WorkspaceGenerator.Import
{
    internal sealed class ImportJobResult
    {
        private readonly List<string> _errors;

        public IReadOnlyList<string> Errors => _errors.AsReadOnly();
        public bool Success => !Errors.Any();

        public ImportJobResult(List<string> errors)
        {
            _errors = errors;
        }

        public override string ToString()
        {
            StringBuilder errorMessageBuilder = new StringBuilder();
            for (int i = 0; i < _errors.Count; i++)
            {
                errorMessageBuilder
                    .AppendFormat(CultureInfo.InvariantCulture, "{0}. {1}", i + 1, _errors[i])
                    .AppendLine();
            }
            return errorMessageBuilder.ToString();
        }
    }
}