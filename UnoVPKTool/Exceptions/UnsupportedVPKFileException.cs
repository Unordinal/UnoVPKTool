using System;
using System.Runtime.Serialization;

namespace UnoVPKTool.Exceptions
{
    /// <summary>
    /// An exception that occurs when a VPK file being read is not supported by this library, most likely due to it being an older or newer version.
    /// </summary>
    public class UnsupportedVPKFileException : VPKException
    {
        public UnsupportedVPKFileException() : base("The VPK file is not a supported type.") { }

        public UnsupportedVPKFileException(string? message) : base(message) { }

        public UnsupportedVPKFileException(string? message, Exception? innerException) : base(message, innerException) { }

        protected UnsupportedVPKFileException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}