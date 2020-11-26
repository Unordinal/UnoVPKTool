using System;
using System.Runtime.Serialization;

namespace LzhamWrapper.Exceptions
{
    public class LzhamException : Exception
    {
        public LzhamException() : base("An Lzham exception occurred.") { }

        public LzhamException(string? message) : base(message) { }

        public LzhamException(string? message, Exception? innerException) : base(message, innerException) { }

        protected LzhamException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}