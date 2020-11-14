using System;
using System.Runtime.Serialization;

namespace UnoVPKTool.Exceptions
{
    /// <summary>
    /// An exception that occurs when a file being read is determined to not be a valid VPK file.
    /// </summary>
    public class InvalidVPKFileException : VPKException
    {
        public InvalidVPKFileException() : base("The VPK file is invalid.") { }

        public InvalidVPKFileException(string? message) : base(message) { }

        public InvalidVPKFileException(string? message, Exception? innerException) : base(message, innerException) { }

        protected InvalidVPKFileException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}