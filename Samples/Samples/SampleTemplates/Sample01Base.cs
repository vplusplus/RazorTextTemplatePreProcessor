
using System.IO;
using System.Text;

namespace Samples.SampleTemplates
{
    public abstract class Sample01Base
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

        protected void WriteLiteral(object something)
        {
            Output.Write(something);
        }

        protected void Write(object something)
        {
            // NOTE: 
            // You MUST consider HtmlEncoding 
            // if the template is used to present user entered content.
            WriteLiteral(something);
        }

        protected abstract void Execute();
    }
}
