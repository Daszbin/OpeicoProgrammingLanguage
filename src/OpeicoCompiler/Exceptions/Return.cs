namespace OpeicoCompiler.Exceptions
{
    internal class Return : JRuntimeException
    {
        internal object? value;
        internal Return(object? value)
            : base("Ret")
        {
            this.value = value;
        }
    }
}
