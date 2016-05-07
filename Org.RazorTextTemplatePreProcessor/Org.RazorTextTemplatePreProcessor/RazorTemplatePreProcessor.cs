
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Org.RazorTextTemplatePreProcessor
{
    [
        ComVisible(true),
        Guid("585254D6-E0BD-40A7-8BCF-201605011000"),
        CodeGenerator(typeof(RazorTextTemplatePreProcessor), "RTT", ProjectSystem.CSharp),    // Generators\{PRJSYSTEM}\MFG
        CodeGenerator(typeof(RazorTextTemplatePreProcessor), "RTT", ProjectSystem.VB),        // Generators\{PRJSYSTEM}\MFG
        CodeGenerator(typeof(RazorTextTemplatePreProcessor), "RTT", ProjectSystem.ASPNet),    // Generators\{PRJSYSTEM}\MFG
        ProvideObject(typeof(RazorTextTemplatePreProcessor))                                  // CLSID\{GUID}
    ]
    public class RazorTextTemplatePreProcessor : BaseCodeGenerator
    {
        public RazorTextTemplatePreProcessor()
        {
            //
        }

        const string TextTemplateDotCshtml = "TextTemplate.cshtml";
        const string HtmlTemplateDotCshtml = "HtmlTemplate.cshtml";

        protected override void Generate(string inputFileName, string inputFileContent, string defaultNamespace, out string defaultOutput, out string defaultOutputExtension)
        {
            if (null == inputFileName) throw new ArgumentNullException("inputFileName");
            if (null == inputFileContent) throw new ArgumentNullException("inputFileContent");

            if (inputFileName.EndsWith(TextTemplateDotCshtml, StringComparison.OrdinalIgnoreCase))
            {
                defaultOutput = TextTemplateSource.Value.Replace("NAMESPACE", defaultNamespace);
                defaultOutputExtension = ".gen.cs";
            }
            else if (inputFileName.EndsWith(HtmlTemplateDotCshtml, StringComparison.OrdinalIgnoreCase))
            {
                defaultOutput = HtmlTemplateSource.Value.Replace("NAMESPACE", defaultNamespace);
                defaultOutputExtension = ".gen.cs";
            }
            else
            {
                bool success = MyRazorTemplateCodeGenerator.Generate(inputFileName, defaultNamespace, out defaultOutput);
                defaultOutputExtension = success ? ".gen.cs" : ".gen.err";
            }
        }

        //.........................................................................................
        #region TextTemplateSource, HtmlTemplateSource
        //.........................................................................................
        public static readonly Lazy<string> TextTemplateSource = new Lazy<string>
        (
            () => ReadEmbeddedResource("TextTemplate.cs")
        );

        public static readonly Lazy<string> HtmlTemplateSource = new Lazy<string>
        (
            () => ReadEmbeddedResource("HtmlTemplate.cs")
        );

        private static readonly Assembly MyAssembly = MethodBase.GetCurrentMethod().DeclaringType.Assembly;

        private static string ReadEmbeddedResource(string resourcePartialName)
        {
            var resourceName = MyAssembly
                .GetManifestResourceNames()
                .Where(x => x.EndsWith(resourcePartialName, StringComparison.OrdinalIgnoreCase))
                .Single();

            using (var stream = MyAssembly.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream))
                return reader.ReadToEnd();
        }

        #endregion

    }
}
