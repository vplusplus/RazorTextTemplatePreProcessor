namespace Samples.SampleTemplates
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    public partial class Sample02 : Sample02Base
    {
        public static Action<System.IO.TextWriter> Title(string title) {
            return new Action<System.IO.TextWriter>(__razor_helper_writer => {
            WriteLiteralTo(__razor_helper_writer, "    <h1>");
            WriteTo(__razor_helper_writer, title);
            WriteLiteralTo(__razor_helper_writer, "</h1>\r\n");
            });
}
        public static Action<System.IO.TextWriter> SomeDate(DateTime something)
{
            return new Action<System.IO.TextWriter>(__razor_helper_writer => {
            WriteLiteralTo(__razor_helper_writer, "    <h2>");
            WriteTo(__razor_helper_writer, something);
            WriteLiteralTo(__razor_helper_writer, "</h2>\r\n");
            });
}
        public Sample02()
        {
        }
        protected override void Execute()
        {
            WriteLiteral("\r\n");
            Write(Title("Hello World"));
            WriteLiteral("\r\n\r\n");
            Write(SomeDate(DateTime.Now));
            WriteLiteral("\r\n\r\n");
            WriteLiteral("\r\n");
        }
    }
}
