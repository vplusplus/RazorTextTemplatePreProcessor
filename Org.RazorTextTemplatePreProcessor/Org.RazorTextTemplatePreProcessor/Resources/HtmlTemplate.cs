#pragma warning disable 1591
namespace NAMESPACE
{
    #region using...
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    #endregion

    //.............................................................................................
    #region HtmlRenderingBase - Essential signatures to write string, object and Action<TextWriter> to Output
    //.............................................................................................
    public abstract partial class HtmlRenderingBase
    {
        protected static void WriteLiteralTo(TextWriter writer, string value)
        {
            if (null != writer && null != value) writer.Write(value);
        }
        protected static void WriteLiteralTo(TextWriter writer, Action<TextWriter> value) // <- Analogous to HelperResult
        {
            if (null != writer && null != value) value(writer);
        }
        protected static void WriteLiteralTo(TextWriter writer, object value)
        {
            if (null != writer && null != value) writer.Write(Convert.ToString(value));
        }
        protected static void WriteTo(TextWriter writer, string value) 
        {
            WriteLiteralTo(writer, WebUtility.HtmlEncode(value));
        }
        protected static void WriteTo(TextWriter writer, Action<TextWriter> value)
        {
            WriteLiteralTo(writer, value);
        }
        protected static void WriteTo(TextWriter writer, object value)
        {
            if (null != value) WriteTo(writer, Convert.ToString(value));
        }

        protected void WriteLiteral(string value)               { WriteLiteralTo(Output, value); }
        protected void WriteLiteral(Action<TextWriter> value)   { WriteLiteralTo(Output, value); }
        protected void WriteLiteral(object value)               { WriteLiteralTo(Output, value); }
        protected void Write(string value)                      { WriteTo(Output, value);        }
        protected void Write(Action<TextWriter> value)          { WriteTo(Output, value);        }
        protected void Write(object value)                      { WriteTo(Output, value);        }

        protected abstract TextWriter Output { get; }
    }

    #endregion

    //.............................................................................................
    #region HtmlTemplate - BaseClass for all HtmlTemplate(s) with support for nested Layout(s) and named-sections.
    //.............................................................................................
    public abstract partial class HtmlTemplate : HtmlRenderingBase
    {
        //.........................................................................................
        #region CreateTemplate() - Stock implementation of default template locator.
        //.........................................................................................
        private static readonly ConcurrentDictionary<string, Type> KnownTemplates = new ConcurrentDictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

        private static HtmlTemplate CreateTemplate(string templateName)
        {
            if (null == templateName) throw new ArgumentNullException("templateName");

            var type = KnownTemplates.GetOrAdd(templateName, FindTemplate);

            try {
                return (HtmlTemplate)Activator.CreateInstance(type);
            }
            catch (Exception err) {
                throw new Exception("Error creating an instance of the template: " + templateName, err);
            }
        }

        private static Type FindTemplate(string templateName)
        {
            if (null == templateName) throw new ArgumentNullException("templateName");

            string[] SystemAssemblies = { typeof(string).Assembly.FullName, "System", "Microsoft", "NewtonSoft" };

            // ~/Path1/Path2/ViewName.cshtml becomes Path1.Path2.ViewName
            var typePartialName = templateName.Trim().Replace('\\', '.').Replace('/', '.').TrimStart('~').Trim('.').Replace(".cshtml", "");

            // NOTE: Assumes the assemblies containing the views ar ealready loaded.
            var matches = AppDomain.CurrentDomain.GetAssemblies()
                .Where(x => !SystemAssemblies.Any(n => x.FullName.StartsWith(n, StringComparison.OrdinalIgnoreCase)))
                .SelectMany(x => x.GetTypes())
                .Where(x => x.FullName.EndsWith(typePartialName, StringComparison.OrdinalIgnoreCase))
                .Where(x => !x.IsAbstract)
                .Where(x => typeof(HtmlTemplate).IsAssignableFrom(x))
                .Where(x => null != x.GetConstructor(Type.EmptyTypes))
                .Take(2)
                .ToList();

            if (0 == matches.Count) throw new Exception("Template not found: " + templateName);
            else if (1 == matches.Count) return matches[0];
            else throw new Exception("More than ONE template matched given name: " + templateName);
        }

        #endregion

        //.........................................................................................
        #region Data structures: DynamicDictionary, Context and NestedContext 
        //.........................................................................................
        private sealed class DynamicDictionary : DynamicObject
        {
            IDictionary<string, object> dictionary = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            /// <summary>
            /// Returns value of suggested member, or NULL if such member is not present.
            /// Member name is NOT case-sensitive.
            /// </summary>
            public override bool TryGetMember(GetMemberBinder binder, out object result)
            {
                return dictionary.TryGetValue(binder.Name, out result);
            }

            /// <summary>
            /// Adds (replace) value of suggested member.
            /// Member name is NOT case-sensitive.
            /// </summary>
            public override bool TrySetMember(SetMemberBinder binder, object value)
            {
                dictionary[binder.Name] = value; return true;
            }
        }

        /// <summary>
        /// Represents target of template's output, including named sections. 
        /// </summary>
        private sealed class Context
        {
            public Context(Context next = null)
            {
                this.Next = next;
            }

            /// <summary>
            /// Points to the next (inner) template in the chain.
            /// </summary>
            public readonly Context Next = null;

            /// <summary>
            /// Optional name of the layout (parent or outer) template
            /// </summary>
            public string Layout = null;

            /// <summary>
            /// Buffer that holds generated output of current template.
            /// </summary>
            public readonly StringWriter Output = new StringWriter(new StringBuilder(1024));

            /// <summary>
            /// Named sections defined in current template.
            /// </summary>
            public readonly IDictionary<string, Action> Sections = new Dictionary<string, Action>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Provides a common anchor for gathering generated output 
        /// from multiple-templates associated with Layout property.
        /// </summary>
        private sealed class NestedContext
        {
            private int __level = 0;

            /// <summary>
            /// Creates a starting point of nested context.
            /// </summary>
            public NestedContext()
            {
                this.Current = new Context();
                this.ViewBag = new DynamicDictionary();
            }

            /// <summary>
            /// Creates another parent context.
            /// Current context is linked as 'Next' context.
            /// There is NO Pop().
            /// </summary>
            public NestedContext Push()
            {
                if (++__level > 10) throw new Exception("Too many levels of nesting. Possible cause: Templates have a circular Layout reference.");
                this.Current = new Context(this.Current);
                return this;
            }

            /// <summary>
            /// The view bag associated with this rendering session.
            /// ViewBag is shared by all templates, as such, the view bag is at the nested context.
            /// </summary>
            public dynamic ViewBag { get; private set; }

            /// <summary>
            /// The outer-most context of this nested context.
            /// </summary>
            public Context Current { get; private set; }
        }

        #endregion

        //.........................................................................................
        #region RenderingContext - A NestedContext with access to current Output, Layout, Sections and a shared ViewBag
        //.........................................................................................
        private NestedContext __renderingContext = null;

        private NestedContext RenderingContext
        {
            get { if (null == __renderingContext) throw new Exception("RenderingContext was NULL."); return __renderingContext; }
            set { __renderingContext = value; }
        }

        protected string Layout
        {
            get { return this.RenderingContext.Current.Layout; }
            set { this.RenderingContext.Current.Layout = value; }
        }

        protected override TextWriter Output
        {
            get { return this.RenderingContext.Current.Output; }
        }

        protected dynamic ViewBag
        {
            get { return this.RenderingContext.ViewBag; }
        }

        #endregion

        //.........................................................................................
        #region DefineSection(), IsSectionDefined(), RenderSection(), RenderBody()
        //.........................................................................................
        protected void DefineSection(string sectionName, Action sectionContent)
        {
            if (null == sectionName) throw new ArgumentNullException("sectionName");
            if (null == sectionContent) throw new ArgumentNullException("sectionContent");

            var sections = this.RenderingContext.Current.Sections;
            if (sections.ContainsKey(sectionName)) throw new Exception("Duplicate SectionName: " + sectionName);
            sections.Add(sectionName, sectionContent);
        }

        protected bool IsSectionDefined(string sectionName)
        {
            if (null == sectionName) throw new ArgumentNullException("sectionName");

            var ctx = RenderingContext.Current;
            while (null != ctx)
            {
                if (ctx.Sections.ContainsKey(sectionName)) return true;
                ctx = ctx.Next;
            }
            return false;
        }

        protected object RenderSection(string sectionName, bool required = true)
        {
            if (null == sectionName) throw new ArgumentNullException("sectionName");

            var found = false;
            var ctx = RenderingContext.Current;
            while (null != ctx)
            {
                if (ctx.Sections.ContainsKey(sectionName))
                {
                    ctx.Sections[sectionName]();    // << Easy... Note the () at the end.
                    ctx.Sections.Remove(sectionName);
                    found = true;
                }
                ctx = ctx.Next;
            }

            if (required && !found)
            {
                var errMsg = string.Format("Required section '{0}' not defined (or already rendered).", sectionName);
                throw new Exception(errMsg);
            }

            return null;
        }

        protected object RenderBody()
        {
            var outer = RenderingContext.Current;
            var inner = RenderingContext.Current.Next;
            if (null == inner) throw new Exception("Call to RenderBody() is valid ONLY from with-in a layout.");

            this.WriteLiteral(inner.Output);    // Render BODY of inner template
            inner.Output.Dispose();             // Dispose, so that, it can't be rendered again
            return null;
        }

        #endregion

        //.........................................................................................
        #region Render(), Render(renderingContext), Execute()
        //.........................................................................................
        public string Render()
        {
            return this.Render(new NestedContext());
        }

        private string Render(NestedContext renderingContext)
        {
            if (null == renderingContext) throw new ArgumentNullException("renderingContext");

            lock (this)
            {
                try
                {
                    // Prep for rendering
                    this.RenderingContext = renderingContext;
                    this.Layout = null;

                    // Render THIS template.
                    // This may optionally setup Layout name and zero-or-more named sections.
                    this.Execute();

                    // Render layout if specified, or return current output.
                    return string.IsNullOrWhiteSpace(this.Layout)
                        ? this.Output.ToString()
                        : CreateTemplate(this.Layout).Render(this.RenderingContext.Push());
                }
                finally
                {
                    this.Layout = null;
                    this.RenderingContext = null;
                }
            }
        }

        /// <summary>
        /// Following signature is implemented by code generated from Razor template.
        /// </summary>
        protected abstract void Execute();

        #endregion

    }

    #endregion

    //.............................................................................................
    #region HtmlTemplate<TModel> - Template with Model support
    //.............................................................................................
    public abstract class HtmlTemplate<TModel> : HtmlTemplate
    {
        /// <summary>
        /// If you need intellisense on this, you are in a wrong business...
        /// </summary>
        public TModel Model
        {
            get; set;
        }

        public string Render(TModel model)
        {
            try {
                this.Model = model;
                return this.Render();
            }
            finally {
                this.Model = default(TModel);
            }
        }
    }

    #endregion
}
#pragma warning restore 1591
