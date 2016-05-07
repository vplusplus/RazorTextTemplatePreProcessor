namespace Samples.SampleTemplates
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    public partial class Sample01 : Sample01Base
    {
        public Sample01()
        {
        }
        protected override void Execute()
        {
            WriteLiteral("\r\n");
            WriteLiteral("<h1>Hello World</h1>\r\n<div>Now: ");
            Write(DateTime.Now);
            WriteLiteral("</div>\r\n");
        }
    }
}
