
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Mvc.Razor;
using System.Web.Razor;
using System.Web.Razor.Generator;
using System.Xml.Linq;

namespace Org.RazorTextTemplatePreProcessor
{
    internal static class MyRazorHostFactory
    {
        internal static RazorEngineHost CreateHost(MyRazorDirectives directives)
        {
            if (directives.GenerateHelperPage)
                return new MyRazorHelperHost(directives);
            else 
                return new MyRazorTextTemplateHost(directives);
        }

        //.........................................................................................
        // This code generation uses Action<TextWriter> in-place-of HelperResult.
        // The template base-class MUST support following signatures (in place of HelperResult)
        //.........................................................................................
        //  void WriteLiteralTo(TextWriter writer, Action<TextWriter> writeAction)
        //  void WriteTo(TextWriter writer, Action<TextWriter> writeAction)
        //  void WriteLiteral(Action<TextWriter> writeAction) 
        //  void Write(Action<TextWriter> writeAction)
        //.........................................................................................
        const string MyHelperResultClassName = "Action<System.IO.TextWriter>";
        const string MyDefaultBaseClassName = "HtmlTemplate";

        private static readonly string[] DefaultNameSpaces =
        {
                "System",
                "System.Collections.Generic",
                "System.IO",
                "System.Linq",
                "System.Text",
                "System.Threading.Tasks",
        };

        //.........................................................................................
        #region  Custom razor hosts: MyRazorTextTemplateHost, MyRazorHelperHost
        //.........................................................................................
        private sealed class MyRazorTextTemplateHost : MvcWebPageRazorHost
        {
            private readonly MyRazorDirectives Directives = null;

            public MyRazorTextTemplateHost(MyRazorDirectives directives) : base(directives.VirtualPath, directives.PhysicalPath)
            {
                this.Directives = directives;

                //.......................................................
                // Generated class context is a STRUCT, not a class.
                // Changing property value will NO effect, unless
                // the this.GeneratedClassContext is set the changed structure.
                //.......................................................
                var ctx = this.GeneratedClassContext;
                ctx.TemplateTypeName = MyHelperResultClassName;
                this.GeneratedClassContext = ctx;

                this.DefaultBaseClass = MyDefaultBaseClassName;

                this.NamespaceImports.Clear();
                foreach (var item in DefaultNameSpaces) this.NamespaceImports.Add(item);
                foreach (var item in ReadDefaultImportsFromNearestWebConfig(directives.PhysicalPath)) this.NamespaceImports.Add(item);

                this.StaticHelpers = true;
                this.EnableInstrumentation = false;
            }

            public override void PostProcessGeneratedCode(CodeGeneratorContext context)
            {
                base.PostProcessGeneratedCode(context);

                context
                    .RemoveApplicationInstanceProperty()
                    .ChangeVisibility(Directives.Visibility("public"))
                    .GeneratedClassIsPartial()
                    .ExecuteMethodIsProtected()
                    ;
            }

        }

        private sealed class MyRazorHelperHost : MvcWebPageRazorHost
        {
            private readonly MyRazorDirectives Directives = null;

            public MyRazorHelperHost(MyRazorDirectives directives) : base(directives.VirtualPath, directives.PhysicalPath)
            {
                this.Directives = directives;

                //.......................................................
                // Generated class context is a STRUCT, not a class.
                // Changing property value will NO effect, unless
                // the this.GeneratedClassContext is set the changed structure.
                //.......................................................
                var ctx = this.GeneratedClassContext;
                ctx.TemplateTypeName = MyHelperResultClassName;
                this.GeneratedClassContext = ctx;

                this.DefaultBaseClass = MyDefaultBaseClassName;

                this.NamespaceImports.Clear();
                foreach (var item in DefaultNameSpaces) this.NamespaceImports.Add(item);
                foreach (var item in ReadDefaultImportsFromNearestWebConfig(directives.PhysicalPath)) this.NamespaceImports.Add(item);

                this.StaticHelpers = true;
                this.EnableInstrumentation = false;
            }

            public override void PostProcessGeneratedCode(CodeGeneratorContext context)
            {
                base.PostProcessGeneratedCode(context);

                context
                    .RemoveBaseClass()
                    .RemoveCTOR()
                    .GeneratedClassIsPartial()
                    .ChangeVisibility(Directives.Visibility("public"))
                    .RemoveApplicationInstanceProperty()
                    .RemoveExecuteMethod()
                    .InjectJustEnoughWriteMethods()
                    // Make generated class static - Which can't be achieved via CodeTypeDeclaration
                    ;
            }
        }

        #endregion

        //.........................................................................................
        #region Post generation decorators
        //.........................................................................................
        private static CodeGeneratorContext RemoveBaseClass(this CodeGeneratorContext context)
        {
            context.GeneratedClass.BaseTypes.Clear();
            return context;
        }

        private static CodeGeneratorContext ChangeVisibility(this CodeGeneratorContext context, string visibility)
        {
            visibility = (visibility ?? "public").Trim().ToLower();

            var generatedClass = context.GeneratedClass;

            switch (visibility)
            {
                case "internal":
                    generatedClass.TypeAttributes = generatedClass.TypeAttributes & ~TypeAttributes.VisibilityMask | TypeAttributes.NestedFamANDAssem;
                    break;

                case "":
                case "public":
                    generatedClass.TypeAttributes = generatedClass.TypeAttributes & ~TypeAttributes.VisibilityMask | TypeAttributes.Public;
                    break;
            }

            return context;
        }

        private static CodeGeneratorContext RemoveCTOR(this CodeGeneratorContext context)
        {
            var generatedClass = context.GeneratedClass;

            var ctor = generatedClass.Members.OfType<CodeConstructor>().FirstOrDefault();
            if (null != ctor) generatedClass.Members.Remove(ctor);

            return context;
        }

        private static CodeGeneratorContext GeneratedClassIsPartial(this CodeGeneratorContext context)
        {
            var generatedClass = context.GeneratedClass;

            generatedClass.IsPartial = true;
            return context;
        }

        private static CodeGeneratorContext ExecuteMethodIsProtected(this CodeGeneratorContext context)
        {
            var generatedClass = context.GeneratedClass;
            context.TargetMethod.Attributes = MemberAttributes.Override | MemberAttributes.Family;
            return context;
        }

        private static CodeGeneratorContext RemoveExecuteMethod(this CodeGeneratorContext context)
        {
            var generatedClass = context.GeneratedClass;
            generatedClass.Members.Remove(context.TargetMethod);
            return context;
        }

        private static CodeGeneratorContext RemoveApplicationInstanceProperty(this CodeGeneratorContext context)
        {
            var generatedClass = context.GeneratedClass;

            var appInstanceProperty = generatedClass
                .Members
                .OfType<CodeMemberProperty>().Where(x => x.Name.Equals("ApplicationInstance"))
                .FirstOrDefault();

            if (null != appInstanceProperty) generatedClass.Members.Remove(appInstanceProperty);

            return context;
        }

        private static CodeGeneratorContext InjectJustEnoughWriteMethods(this CodeGeneratorContext context)
        {
            const string writeObjectLiteralTo = @"
            if (null != writer && null != value) writer.Write(Convert.ToString(value));
            ";

            const string writeStringLiteralTo = @"
            if (null != writer && null != value) writer.Write(value);
            ";

            const string writeActionLiteralTo = @"
            if (null != writer && null != value) value(writer);
            ";

            const string writeTo = @"
            if (null != writer && null != value) WriteLiteralTo(writer, value);
            ";

            context.GeneratedClass
                .AddPrivateStaticMetod<TextWriter, object>("WriteLiteralTo", "writer", "value", writeObjectLiteralTo)
                .AddPrivateStaticMetod<TextWriter, string>("WriteLiteralTo", "writer", "value", writeStringLiteralTo)
                .AddPrivateStaticMetod<TextWriter, Action<TextWriter>>("WriteLiteralTo", "writer", "value", writeActionLiteralTo)
                .AddPrivateStaticMetod<TextWriter, object>("WriteTo", "writer", "value", writeTo)
                .AddPrivateStaticMetod<TextWriter, string>("WriteTo", "writer", "value", writeTo)
                .AddPrivateStaticMetod<TextWriter, Action<TextWriter>>("WriteTo", "writer", "value", writeTo)
                ;

            return context;
        }

        private static CodeTypeDeclaration AddPrivateStaticMetod<TArg1, TArg2>(this CodeTypeDeclaration generatedClass, string methodName, string arg1Name, string arg2Name, string body)
        {
            var method = new CodeMemberMethod()
            {
                Name = methodName,
                Attributes = MemberAttributes.Private | MemberAttributes.Static,
            };
            method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(TArg1), arg1Name));
            method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(TArg2), arg2Name));
            method.Statements.Add(new CodeSnippetStatement(body));
            generatedClass.Members.Add(method);

            return generatedClass;
        }

        #endregion

        //.........................................................................................
        #region ReadDefaultImportsFromNearestWebConfig()
        //.........................................................................................
        private static IEnumerable<string> ReadDefaultImportsFromNearestWebConfig(string inputFileName)
        {
            if (null == inputFileName) throw new ArgumentNullException("inputFileName");

            var configFile = FindMyNearestWebConfig(inputFileName);

            return null == configFile ? Enumerable.Empty<string>()
                : XDocument
                    .Load(configFile)
                    .Root
                    .Descendants("pages")
                    .Take(1)
                    .Elements("namespaces")
                    .Take(1)
                    .Elements("add")
                    .Select(x => (string)x.Attribute("namespace"))
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToArray();
        }

        private static string FindMyNearestWebConfig(string someFileName)
        {
            if (null == someFileName) throw new ArgumentNullException("someFileName");

            var currentDir = Path.GetDirectoryName(someFileName);

            while (null != currentDir)
            {
                var webConfigFileName = Directory.GetFiles(currentDir, "web.config").FirstOrDefault();
                if (null != webConfigFileName) return webConfigFileName;

                var isSolutionFolder = Directory.GetFiles(currentDir, "*.sln").Length > 0;
                if (isSolutionFolder) return null;

                currentDir = Path.GetDirectoryName(currentDir);
            }

            return null;
        }

        #endregion

    }
}
