using System;
using System.Runtime.Serialization;

namespace UnoVPKTool.Exceptions
{
    /// <summary>
    /// A generic VPK-related exception.
    /// </summary>
    public class VPKException : Exception
    {
        public VPKException() : base("There was an error with the VPK.") { }

        public VPKException(string? message) : base(message) { }

        public VPKException(string? message, Exception? innerException) : base(message, innerException) { }

        protected VPKException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}