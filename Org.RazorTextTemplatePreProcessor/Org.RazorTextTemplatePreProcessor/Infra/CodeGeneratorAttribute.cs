
//.............................................................................................
//  Attribute for registering single file generator
//  Can create pkgdef entries for [Generators\{ProjectSystemGuid}\GeneratorName]
//  Required dependency: Microsoft.VisualStudio.Shell.Immutable.10.0
//.............................................................................................

using System;
using Microsoft.VisualStudio.Shell;

namespace Org.RazorTextTemplatePreProcessor
{
    /// <summary>
    /// Identifies target projectsystem.
    /// </summary>
    public enum ProjectSystem
    {
        CSharp = 1,
        VB = 2,
        ASPNet = 3,
    }

    /// <summary>
    /// GUIDs corresponding to each project system.
    /// </summary>
    static class ProjectSystemGuids
    {
        const string CSharp = "{fae04ec1-301f-11d3-bf4b-00c04f79efbc}";
        const string VB = "{164b10b9-b200-11d0-8c61-00a0c91e29d5}";
        const string ASPNet = "{39c9c826-8ef8-4079-8c95-428f5b1c323f}";

        public static string GuidForProjectSystem(ProjectSystem projectSystem)
        {
            switch (projectSystem)
            {
                case ProjectSystem.CSharp: return CSharp;
                case ProjectSystem.VB: return VB;
                case ProjectSystem.ASPNet: return ASPNet;
                default: throw new Exception("Unknown project system: " + projectSystem);
            }
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public sealed class CodeGeneratorAttribute : Microsoft.VisualStudio.Shell.RegistrationAttribute
    {
        public CodeGeneratorAttribute(Type generatorType, string generatorName, ProjectSystem projectSystem, bool generatesDesignTimeSource = true)
        {
            if (generatorType == null) throw new ArgumentNullException("generatorType");
            if (generatorName == null) throw new ArgumentNullException("generatorName");

            this.GeneratorType = generatorType;
            this.GeneratorName = generatorName;
            this.ProjectSystemGuid = ProjectSystemGuids.GuidForProjectSystem(projectSystem);
            this.GeneratesDesignTimeSource = generatesDesignTimeSource;
        }

        private readonly Type GeneratorType;
        private readonly string GeneratorName;
        private readonly string ProjectSystemGuid;
        private readonly bool GeneratesDesignTimeSource;

        private string RegKey_Generator
        {
            get
            {
                // Generators\ProjectSystemGuid\GeneratorName]
                return string.Format("Generators\\{0}\\{1}", this.ProjectSystemGuid, this.GeneratorName );
            }
        }

        public override void Register(RegistrationAttribute.RegistrationContext context)
        {
            if (null == context) throw new ArgumentNullException("context");

            //............................................................
            // [$RootKey$\Generators\{PROJECT_SYSTEM_GUID}\GeneratoName]
            // @="GeneratorName"
            // "CLSID"="Generator Guid"
            // "GeneratesDesignTimeSource"=dword:00000001
            //............................................................
            using (RegistrationAttribute.Key key = context.CreateKey(this.RegKey_Generator))
            {
                key.SetValue(string.Empty, this.GeneratorName);
                key.SetValue("CLSID", this.GeneratorType.GUID.ToString("B"));
                key.SetValue("GeneratesDesignTimeSource", this.GeneratesDesignTimeSource ? 1 : 0);
            }
        }

        public override void Unregister(RegistrationAttribute.RegistrationContext context)
        {
            if (null == context) throw new ArgumentNullException("context");

            context.RemoveKey(this.RegKey_Generator);
        }
    }
}
