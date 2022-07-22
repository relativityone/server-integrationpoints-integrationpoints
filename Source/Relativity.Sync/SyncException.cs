using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Relativity.Sync
{
    /// <summary>
    ///     Represents Sync exception
    /// </summary>
    [Serializable]
    public class SyncException : Exception
    {
        /// <summary>
        ///     Sync job workflow ID
        /// </summary>
        public string WorkflowId { get; }

        /// <inheritdoc />
        public SyncException()
        {
        }

        /// <inheritdoc />
        protected SyncException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            WorkflowId = info.GetString(nameof(WorkflowId));
        }

        /// <inheritdoc />
        public SyncException(string message) : base(message)
        {
        }

        /// <inheritdoc />
        public SyncException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        ///     Constructor with workflow ID
        /// </summary>
        public SyncException(string message, string workflowId) : base(message)
        {
            WorkflowId = workflowId;
        }

        /// <summary>
        ///     Constructor with workflow ID
        /// </summary>
        public SyncException(string message, Exception innerException, string workflowId) : base(message, innerException)
        {
            WorkflowId = workflowId;
        }

        /// <summary>
        /// </summary>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            info.AddValue(nameof(WorkflowId), WorkflowId);
            base.GetObjectData(info, context);
        }
    }
}