using System;
using System.Runtime.Serialization;

namespace DotFSM
{
    [Serializable]
    internal class FSMDefinitionException : Exception
    {
        public FSMDefinitionException()
        {
        }

        public FSMDefinitionException(string message) : base(message)
        {
        }

        public FSMDefinitionException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected FSMDefinitionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}