
//.............................................................................................
//  Attribute for registering single file generator
//  Can create pkgdef entries for [CLSID\{GUID}]
//  Required dependency: Microsoft.VisualStudio.Shell.Immutable.10.0
//.............................................................................................

using System;
using Microsoft.VisualStudio.Shell;

namespace Org.RazorTextTemplatePreProcessor
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public sealed class ProvideObjectAttribute : RegistrationAttribute
    {
        readonly Type ObjectType;

        public ProvideObjectAttribute(Type objectType)
        {
            if (objectType == null) throw new ArgumentNullException("objectType");

            this.ObjectType = objectType;
        }

        private string CLSIDRegKey
        {
            get
            {
                // CLSID\{GUID}
                return string.Format("CLSID\\{0}", this.ObjectType.GUID.ToString("B"));
            }
        }

        public override void Register(RegistrationAttribute.RegistrationContext context)
        {
            if (null == context) throw new ArgumentNullException("context");

            context.Log.WriteLine("Adding CLSID key and values for " + this.ObjectType.FullName);

            using (RegistrationAttribute.Key key = context.CreateKey(this.CLSIDRegKey))
            {
                key.SetValue(string.Empty, this.ObjectType.FullName);
                key.SetValue("Class", this.ObjectType.FullName);
                key.SetValue("CodeBase", context.CodeBase);
                key.SetValue("InprocServer32", context.InprocServerPath);
                key.SetValue("ThreadingModel", "Both");
            }
        }

        public override void Unregister(RegistrationAttribute.RegistrationContext context)
        {
            if (null == context) throw new ArgumentNullException("context");

            context.RemoveKey(this.CLSIDRegKey);
        }
    }
}
