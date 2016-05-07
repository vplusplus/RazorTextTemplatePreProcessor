#pragma warning disable 1591
namespace NAMESPACE
{
    #region using...

    using System;
    using System.IO;
    using System.Text;

    #endregion

    //.............................................................................................
    #region TextTemplate - Provides essential signatures for writing string, Action<TextWriter> and object
    //.............................................................................................
    public abstract partial class TextTemplate
    {
        private StringWriter Output = null;

        public virtual string Render()
        {
            lock(this)
            {
                try {
                    Output = new StringWriter(new StringBuilder(1024));
                    Execute();
                    return Output.ToString();
                }
                finally {
                    Output = null;
                }
            }
        }

        protected static void WriteLiteralTo(TextWriter writer, string value) { if (null != writer && null != value) writer.Write(value); }
        protected static void WriteLiteralTo(TextWriter writer, Action<TextWriter> value) { if (null != writer && null != value) value(writer); }
        protected static void WriteLiteralTo(TextWriter writer, object value) { if (null != writer && null != value) writer.Write(Convert.ToString(value)); }

        protected static void WriteTo(TextWriter writer, string value) { WriteLiteralTo(writer, value); }
        protected static void WriteTo(TextWriter writer, Action<TextWriter> value) { WriteLiteralTo(writer, value); }
        protected static void WriteTo(TextWriter writer, object value) { WriteLiteralTo(writer, value); }

        protected void WriteLiteral(string value) { WriteLiteralTo(Output, value); }
        protected void WriteLiteral(Action<TextWriter> value) { WriteLiteralTo(Output, value); }
        protected void WriteLiteral(object value) { WriteLiteralTo(Output, value); }

        protected void Write(string value) { WriteLiteralTo(Output, value); }
        protected void Write(Action<TextWriter> value) { WriteLiteralTo(Output, value); }
        protected void Write(object value) { WriteLiteralTo(Output, value); }

        protected abstract void Execute();
    }

    #endregion

    //.............................................................................................
    #region TextTemplate<T> - With model support
    //.............................................................................................
    public abstract partial class TextTemplate<TModel> : TextTemplate
    {
        public TModel Model
        {
            get; set;
        }

        public virtual string Render(TModel model)
        {
            try
            {
                this.Model = model;
                return this.Render();
            }
            finally
            {
                this.Model = default(TModel);
            }
        }
    }
    #endregion

}
#pragma warning restore 1591