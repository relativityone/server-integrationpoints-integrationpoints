using System;

namespace kCura.IntegrationPoints.Domain.Models
{
    public class ErrorDTO : BaseDTO, IEquatable<ErrorDTO>
    {
        /// <summary>
        /// The short and descriptive error message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// The full text for the error.
        /// </summary>
        public string FullText { get; set; }

        /// <summary>
        /// The source of the error.
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// The workspace id the error originated from.
        /// </summary>
        public int WorkspaceId { get; set; }

        /// <summary>
        /// Intentionally not comparing FullText. Stack trace can be similar but not exact.
        /// </summary>
        public bool Equals(ErrorDTO other)
        {
            if (other == null)
            {
                return false;
            }
            bool isCorrectMessage = Message == other.Message;
            bool isCorrectSource = Source == other.Source;
            bool isCorrectWorkspace = WorkspaceId == other.WorkspaceId;
            return isCorrectMessage && isCorrectSource && isCorrectWorkspace;
        }
    }
}