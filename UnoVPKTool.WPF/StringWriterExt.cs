using System;
using System.IO;

namespace UnoVPKTool.WPF
{
    // Taken from https://stackoverflow.com/questions/3708454/is-there-a-textwriter-child-class-that-fires-event-if-text-is-written/3710257#3710257
    public class StringWriterExt : StringWriter
    {
        public virtual bool AutoFlush { get; set; }

        public event EventHandler? Flushed;

        public StringWriterExt(bool autoFlush) : base()
        {
            AutoFlush = autoFlush;
        }

        public StringWriterExt() : base() { }

        public override void Flush()
        {
            base.Flush();
            OnFlushed();
        }

        public override void Write(char value)
        {
            base.Write(value);
            if (AutoFlush) Flush();
        }

        public override void Write(char[] buffer, int index, int count)
        {
            base.Write(buffer, index, count);
            if (AutoFlush) Flush();
        }

        public override void Write(string? value)
        {
            base.Write(value);
            if (AutoFlush) Flush();
        }

        protected void OnFlushed()
        {
            Flushed?.Invoke(this, EventArgs.Empty);
        }
    }
}