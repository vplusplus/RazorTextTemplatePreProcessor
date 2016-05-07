
using System;
using System.IO;
using System.Text;

namespace Samples.SampleTemplates
{
    public abstract class Sample02Base
    {
        private TextWriter Output = null;

        public string Render()
        {
            try {
                Output = new StringWriter(new StringBuilder(128));
                Execute();
                return Output.ToString();
            }
            finally {
                Output = null;
            }

        }

        protected static void WriteLiteralTo(TextWriter output, object something)
        {
            output.Write(something);
        }

        protected static void WriteTo(TextWriter output, object something)
        {
            // NOTE: 
            // You MUST consider HtmlEncoding 
            // if the template is used to present user entered content.
            WriteLiteralTo(output, something);
        }

        protected void WriteLiteral(object something)
        {
            WriteLiteralTo(this.Output, something);
        }

        protected void Write(object something)
        {
            // NOTE: 
            // You MUST consider HtmlEncoding 
            // if the template is used to present user entered content.
            WriteLiteralTo(this.Output, something);
        }

        protected void Write(Action<TextWriter> writeAction)
        {
            // By default Razor would expect HelperResult here.
            // To minimize moving parts, the RTT custom tool, instead generates Action<TextWriter> in place of HelperResult.
            writeAction(this.Output);
        }

        protected abstract void Execute();
    }
}
