
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Org.RazorTextTemplatePreProcessor;

namespace UnitTests
{
    [TestClass]
    public class UnitTest1
    {

        [TestMethod]
        public void GenerateSampleTextTemplate()
        {
            var inputFileName = "../../Samples/SampleTextTemplate.cshtml";
            var defaultNameSpace = "a.b.c";

            string output = null;
            var success = MyRazorTemplateCodeGenerator.Generate(inputFileName, defaultNameSpace, out output);
            Console.WriteLine(output);
        }

        [TestMethod]
        public void GenerateSampleHtmlTemplate()
        {
            var inputFileName = "../../Samples/SampleHtmlTemplate.cshtml";
            var defaultNameSpace = "a.b.c";

            string output = null;
            var success = MyRazorTemplateCodeGenerator.Generate(inputFileName, defaultNameSpace, out output);
            Console.WriteLine(output);
        }

        [TestMethod]
        public void GenerateHelper()
        {
            var inputFileName = "../../Samples/SampleHelpers.cshtml";
            var defaultNameSpace = "a.b.c";

            string output = null;
            var success = MyRazorTemplateCodeGenerator.Generate(inputFileName, defaultNameSpace, out output);
            Console.WriteLine(output);
        }
    }
}
