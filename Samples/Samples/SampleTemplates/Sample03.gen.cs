namespace Samples.SampleTemplates
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    public partial class Sample03 : Sample03Base
    {
        public static Action<System.IO.TextWriter> RenderRow(Type t)
{
            return new Action<System.IO.TextWriter>(__razor_helper_writer => {
            WriteLiteralTo(__razor_helper_writer, "    <tr>\r\n        <td>");
            WriteTo(__razor_helper_writer, t.Namespace);
            WriteLiteralTo(__razor_helper_writer, "</td>\r\n        <td>");
            WriteTo(__razor_helper_writer, t.Name);
            WriteLiteralTo(__razor_helper_writer, "</td>\r\n        <td>");
            WriteTo(__razor_helper_writer, t.Assembly.GetName().Name);
            WriteLiteralTo(__razor_helper_writer, "</td>\r\n    </tr>\r\n");
            });
}
        public Sample03()
        {
        }
        protected override void Execute()
        {
            WriteLiteral("\r\n\r\n");
    var types = AppDomain
        .CurrentDomain
        .GetAssemblies()
        .SelectMany(x => x.GetTypes())
        .ToList()
        ;
            WriteLiteral("\r\n\r\n<h1>Types currently loaded...</h1>\r\n\r\n<table>\r\n    <thead>\r\n        <tr>\r\n   " +
"         <th>Namespace</th>\r\n            <th>Name</th>\r\n            <th>Assembly" +
"</th>\r\n        </tr>\r\n    </thead>\r\n    <tbody>\r\n");
         foreach (var t in types)
        {
            Write(RenderRow(t));
        }
            WriteLiteral("    </tbody>\r\n</table>\r\n\r\n");
        }
    }
}
