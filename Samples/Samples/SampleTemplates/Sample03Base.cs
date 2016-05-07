
using System;
using System.IO;
using System.Text;

namespace Samples.SampleTemplates
{
    public abstract class Sample03Base
    {
        private TextWriter Output = null;

        public string Render()
        {
            try
            {
                Output = new StringWriter(new StringBuilder(128));
                Execute();
                return Output.ToString();
            }
            finally
            {
                Output = null;
            }

        }

        //.........................................................................................
        #region 13 signatures that should cover 80% of the self-containted-templates
        //.........................................................................................
        // Consider HtmlEncoding Write() and WriteTo() signatures if
        // a) You are generating user-facing html pages
        // b) Presenting user entered content
        // c) Or else, risk script injection attacks. 
        //.........................................................................................
        protected static void WriteLiteralTo(TextWriter writer, string value)
        {
            if (null != writer && null != value) writer.Write(value);
        }
        protected static void WriteLiteralTo(TextWriter writer, object value)
        {
            if (null != writer && null != value) writer.Write(value);
        }
        protected static void WriteLiteralTo(TextWriter writer, /*HelperResult*/ Action<TextWriter> writeAction)
        {
            if (null != writer && null != writeAction) writeAction(writer); 
        }

        protected static void WriteTo(TextWriter writer, string value) { WriteLiteralTo(writer, value); }
        protected static void WriteTo(TextWriter writer, object value) { WriteLiteralTo(writer, value); }
        protected static void WriteTo(TextWriter writer, Action<TextWriter> value) { WriteLiteralTo(writer, value); }

        protected void WriteLiteral(string value) { WriteLiteralTo(this.Output, value); }
        protected void WriteLiteral(object value) { WriteLiteralTo(this.Output, value); }
        protected void WriteLiteral(Action<TextWriter> value) { WriteLiteralTo(this.Output, value); }

        protected void Write(string value) { WriteTo(this.Output, value); }
        protected void Write(object value) { WriteTo(this.Output, value); }
        protected void Write(Action<TextWriter> value) { WriteTo(this.Output, value); }

        // Implemented by generated code.
        protected abstract void Execute();

        #endregion
    }
}
