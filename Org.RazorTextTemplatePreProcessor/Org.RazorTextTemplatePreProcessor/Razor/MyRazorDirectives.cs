using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Org.RazorTextTemplatePreProcessor
{
    internal sealed class MyRazorDirectives
    {
        private readonly string InputFileName = null;
        private readonly IDictionary<string, string> Directives = null;

        internal MyRazorDirectives(string inputFileName)
        {
            this.InputFileName = Path.GetFullPath(inputFileName);
            this.Directives = ReadDirectives(inputFileName);
        }

        public string PhysicalPath
        {
            get { return InputFileName; }
        }

        public string VirtualPath
        {
            get { return InputFileName.Replace(Path.GetPathRoot(InputFileName), "~/").Replace('\\', '/'); }
        }


        // @** HelperPage *@
        public bool GenerateHelperPage
        {
            get {
                return Directives.ContainsKey("HelperPage");
            }
        }

        // @** namespace : CustomNamespace  *@
        public string Namespace(string defaultNamespace)
        {
            string customNamespace = null;
            Directives.TryGetValue("namespace", out customNamespace);
            return string.IsNullOrWhiteSpace(customNamespace) ? defaultNamespace : customNamespace;
        }

        // @** visibility : public | internal *@
        public string Visibility(string defaultVisibility)
        {
            string value = null;
            Directives.TryGetValue("visibility", out value);
            return string.IsNullOrWhiteSpace(value) ? defaultVisibility : value;
        }

        // @** classname : CustomClassName *@
        public string ClassName()
        {
            var defaultClassName = Path.GetFileNameWithoutExtension(InputFileName);
            var customClassName = string.Empty;

            Directives.TryGetValue("classname", out customClassName);
            return string.IsNullOrWhiteSpace(customClassName) ? defaultClassName: customClassName;
        }

        // @** Remove#Lines or Remove#Line *@
        public bool RemoveLinePragmas
        {
            get
            {
                return 
                    Directives.ContainsKey("Remove#Line") || 
                    Directives.ContainsKey("Remove#Lines")
                    ;
            }
        }


        //.........................................................................................
        #region ReadDirectives()
        //.........................................................................................
        // @** DirectiveName: DirectiveValue *@
        //.........................................................................................
        const string DirectivesSyntax = @"
                ^\s*@\*\*           # Starts with @**
                (?<name>[^:]+)      # Directive name
                :?                  # Followed by optional :
                (?<value>[^\*]*)    # Followed by optional value
                \*@\s*$             # Ends with *@
                ";

        private static readonly Regex RxDirectiveLine = new Regex(DirectivesSyntax, RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);

        private static Dictionary<string, string> ReadDirectives(string inputFileName)
        {
            if (null == inputFileName) throw new ArgumentNullException("inputFileName");

            return ReadLines(inputFileName)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Take(20)
                .Select(x => RxDirectiveLine.Match(x))
                .Where(m => m.Success)
                .Select(m => new { Key = m.Groups["name"].Value, Value = m.Groups["value"].Value })
                .Where(x => !string.IsNullOrWhiteSpace(x.Key))
                .ToDictionary(x => x.Key.Trim(), x => (x.Value ?? "").Trim(), StringComparer.OrdinalIgnoreCase)
                ;
        }

        private static IEnumerable<string> ReadLines(string inputFileName)
        {
            using (var reader = File.OpenText(inputFileName))
            {
                string nextLine = null;
                while (null != (nextLine = reader.ReadLine()))
                {
                    if (!string.IsNullOrWhiteSpace(nextLine)) yield return nextLine;
                }
            }
        }

        #endregion

    }
}
