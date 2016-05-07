
using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Razor;
using System.Web.Razor.Parser.SyntaxTree;

namespace Org.RazorTextTemplatePreProcessor
{
    public static class MyRazorTemplateCodeGenerator
    {
        public static bool Generate(string inputFileName, string suggestedNamespace, out string generatedContent)
        {
            if (null == inputFileName) throw new ArgumentNullException("inputFileName");
            if (null == suggestedNamespace) throw new ArgumentNullException("suggestedNamespace");

            inputFileName = Path.GetFullPath(inputFileName);

            var directives = new MyRazorDirectives(inputFileName);

            var host = MyRazorHostFactory.CreateHost(directives);
            var engine = new RazorTemplateEngine(host);

            var generationResults = engine.GenerateCode(
                new StreamReader(inputFileName),
                className: directives.ClassName(),
                rootNamespace: directives.Namespace(suggestedNamespace),
                sourceFileName: Path.GetFileName(inputFileName)
            );

            if (null != generationResults.ParserErrors && generationResults.ParserErrors.Count > 0)
            {
                generatedContent = ParserErrorDetails(generationResults.ParserErrors);
                return false;
            }

            // Create C# source...
            StringBuilder generatedSource = new StringBuilder(1024);
            new CSharpCodeProvider().GenerateCodeFromCompileUnit
            (
                generationResults.GeneratedCode,
                new StringWriter(generatedSource),
                new CodeGeneratorOptions() { BlankLinesBetweenMembers = false, BracingStyle = "C" }
            );

            generatedContent = CleanupGeneratedSource(generatedSource.ToString(), directives.RemoveLinePragmas);

            // Things we can't accomplish via API
            if (directives.GenerateHelperPage)
            {
                generatedContent = generatedContent.Replace("partial class", "static partial class");
            }


            return true;
        }

        //.........................................................................................
        #region CleanupGeneratedSource(), ParserErrorDetails()
        //.........................................................................................
        private static string CleanupGeneratedSource(string uncleanSource, bool removeLinePragmas)
        {
            string[] Add12SpacesIfStartsWith =
            {
                "#line",
                "Write(",
                "WriteTo(",
                "WriteLiteral(",
                "WriteLiteralTo(",
                "WriteAttribute(",
                "WriteAttributeTo(",
                "DefineSection(",
                "});",
                "return new"
            };

            string[] Add8SpacesIfStartsWith =
            {
                "public static Action<System.IO.TextWriter>",
                "public Action<System.IO.TextWriter>",
            };

            // 

            string EightSpaces = new string(' ', 8);
            string TwelveSpaces = new string(' ', 12);

            var cleanSource = new StringBuilder();
            using (var reader = new StringReader(uncleanSource))
            {
                string nextLine = null;
                while (null != (nextLine = reader.ReadLine()))
                {
                    if (string.IsNullOrWhiteSpace(nextLine)) continue;
                    if (nextLine.TrimStart().StartsWith("//")) continue;
                    if (removeLinePragmas && nextLine.TrimStart().StartsWith("#line")) continue;

                    if (Add8SpacesIfStartsWith.Any(x => nextLine.TrimStart().StartsWith(x))) nextLine = EightSpaces + nextLine.TrimStart();
                    if (Add12SpacesIfStartsWith.Any(x => nextLine.TrimStart().StartsWith(x))) nextLine = TwelveSpaces + nextLine.TrimStart();

                    cleanSource.AppendLine(nextLine);
                }
            }

            return cleanSource.ToString();
        }

        private static string ParserErrorDetails(this IList<RazorError> parserErrors)
        {
            StringBuilder buffer = new StringBuilder(512);

            foreach (var error in parserErrors)
            {
                buffer.Append(error.Message).AppendLine();
                buffer.AppendFormat("  at line:{0} col:{1}", error.Location.LineIndex, error.Location.CharacterIndex).AppendLine();
                buffer.AppendLine();
            }

            return buffer.ToString();
        }

        #endregion

    }
}
