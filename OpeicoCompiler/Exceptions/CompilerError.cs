﻿namespace OpeicoCompiler.Exceptions
{
    internal class CompilerError : ICompilerError
    {
        internal List<string> Errors = new();
        internal string sourceFileName = string.Empty;

        public CompilerError()
        {
        }
        void ICompilerError.AddError(string errorMessage)
        {
            Errors.Add(errorMessage);
            //System.Diagnostics.Debugger.Break();
        }

        bool ICompilerError.HasErrors()
        {
            return Errors.Count > 0;
        }

        List<string> ICompilerError.ListErrors()
        {
            return Errors;
        }

        void ICompilerError.SourceFileName(string fileName)
        {
            sourceFileName = fileName;
        }
    }
}
