
//.............................................................................................
//  Required dependency
//.............................................................................................
//  <Reference Include="Microsoft.VisualStudio.Shell.Interop">
//      <SpecificVersion>False</SpecificVersion>
//      <Private>False</Private>
//  </Reference>
//.............................................................................................

using System;
using System.CodeDom.Compiler;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.VisualStudio.Shell.Interop;
using System.Threading;

namespace Org.RazorTextTemplatePreProcessor
{
    /// <summary>
    /// Abstract baseclass that implements IVsSingleFileGenerator.
    /// </summary>
    public abstract class BaseCodeGenerator : Microsoft.VisualStudio.Shell.Interop.IVsSingleFileGenerator, IDisposable
    {
        public BaseCodeGenerator()
        {
            //
        }

        //.........................................................................................
        #region IVsSingleFileGenerator
        //.........................................................................................
        const int S_OK = 0;
        const int E_FAIL = -2147467259;

        private IVsGeneratorProgress GeneratorProgress;
        private string DefaultFileExtension;

        int IVsSingleFileGenerator.DefaultExtension(out string pbstrDefaultExtension)
        {
            pbstrDefaultExtension = this.DefaultFileExtension ?? ".gen";
            return S_OK;
        }

        int IVsSingleFileGenerator.Generate(string wszInputFilePath, string bstrInputFileContents, string wszDefaultNamespace, IntPtr[] rgbOutputFileContents, out uint pcbOutput, IVsGeneratorProgress pGenerateProgress)
        {
            if (wszInputFilePath == null) throw new ArgumentNullException("wszInputFilePath");
            if (bstrInputFileContents == null) throw new ArgumentNullException("bstrInputFileContents");

            string outputFileContent = null;
            string outputFileExtension = null;

            try
            {
                this.GeneratorProgress = pGenerateProgress;
                this.Generate(wszInputFilePath, bstrInputFileContents, wszDefaultNamespace ?? string.Empty, out outputFileContent, out outputFileExtension);
            }
            catch (Exception err)
            {
                // Show errors in Visual Studio Error-List-Window...
                this.InformError(err);

                // Capture exception details...
                var buffer = new StringBuilder();
                while (null != err)
                {
                    buffer.Append(err.Message).AppendLine();
                    buffer.Append("  ").Append(err.GetType().FullName).AppendLine();
                    err = err.InnerException;
                }

                // Capture exception details to .gen.err output file.
                outputFileContent = buffer.ToString();
                outputFileExtension = ".gen.err";
            }
            finally
            {
                this.GeneratorProgress = null;
            }

            if (string.IsNullOrWhiteSpace(outputFileContent))
            {
                rgbOutputFileContents[0] = IntPtr.Zero;
                pcbOutput = 0;
            }
            else
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(outputFileContent);
                int outputLength = bytes.Length;

                rgbOutputFileContents[0] = Marshal.AllocCoTaskMem(outputLength);
                Marshal.Copy(bytes, 0, rgbOutputFileContents[0], outputLength);
                pcbOutput = (uint)outputLength;
            }

            // Remember suggested extension, for future call back from VStudio.
            this.DefaultFileExtension = outputFileExtension;

            // Always return S_OK, because...
            // This implementation captures unhandled exceptions
            // a) Reports to Visual Studio
            // b) Catures errors as default output 
            // If we return E_Fail, the error details WILL NOT be written to .gen.err file.
            return S_OK;
        }

        /// <summary>Must override...</summary>
        protected abstract void Generate(
            string inputFileName, string inputFileContent, string defaultNamespace,
            out string defaultOutput, out string defaultOutputExtension
        );

        #endregion

        //.........................................................................................
        #region Inform errors and warnings...
        //.........................................................................................
        private void GeneratorErrorCallback(bool warning, int level, string message, int line, int column)
        {
            var vsGeneratorProgress = this.GeneratorProgress;

            if (null != message && vsGeneratorProgress != null)
            {
                vsGeneratorProgress.GeneratorError(warning ? -1 : 0, (uint)level, message, (uint)line, (uint)column);
            }
        }

        protected virtual void InformWarning(string message, int line = 0, int column = 0)
        {
            this.GeneratorErrorCallback(warning: true, level: 0, message: message, line: line, column: column);
        }

        protected virtual void InformError(string message, int line = 0, int column = 0)
        {
            this.GeneratorErrorCallback(warning: false, level: 0, message: message, line: line, column: column);
        }

        protected virtual void InformError(Exception error)
        {
            if (null == error) return;
            if (null == this.GeneratorProgress) return;

            var buffer = new StringBuilder();
            while (null != error)
            {
                buffer.AppendLine(error.Message);
                buffer.AppendLine(error.GetType().FullName);
                error = error.InnerException;
            }

            this.InformError(buffer.ToString());
        }

        protected void InformWarningsAndErrors(CompilerErrorCollection errorsAndWarnings)
        {
            if (null == errorsAndWarnings) return;
            if (null == this.GeneratorProgress) return;

            foreach (CompilerError item in errorsAndWarnings)
            {
                if (null == item.ErrorText) continue;
                this.GeneratorErrorCallback(item.IsWarning, 0, item.ErrorText, item.Line, item.Column);
            }
        }

        #endregion

        //.........................................................................................
        #region IDisposable
        //.........................................................................................
        int __disposing = 0;

        protected virtual void Dispose(bool disposing)
        {
            // Placeholder for derrived classes.
        }

        private void DisposeOnce(bool callFromGC)
        {
            var alreadyDisposed = Interlocked.Exchange(ref __disposing, 1) > 0;
            if (alreadyDisposed) return;

            try
            {
                // Allow derrived classes to dospose...
                this.Dispose(callFromGC);
            }
            finally
            {
                // Clean-up
                this.GeneratorProgress = null;
            }
        }

        void IDisposable.Dispose()
        {
            this.DisposeOnce(false);
            GC.SuppressFinalize(this);
        }

        ~BaseCodeGenerator()
        {
            this.DisposeOnce(true);
        }

        #endregion
    }
}
