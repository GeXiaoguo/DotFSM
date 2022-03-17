using System.Runtime.Serialization;

namespace IssueTracker
{
    [Serializable]
    internal class IssueWorkflowException : Exception
    {
        public IssueWorkflowException()
        {
        }

        public IssueWorkflowException(string? message) : base(message)
        {
        }

        public IssueWorkflowException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected IssueWorkflowException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}