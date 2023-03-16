namespace DynamicReflector.Samples
{
    internal class InternalClass
    {
        private int value;

        private static string StaticMethod()
        {
            return "static";
        }

        public InternalClass() 
            : this(0)
        {
        }

        private InternalClass(int value)
        {
            this.value = value;
        }

        private int ReturnValue()
        {
            return this.value;
        }
    }
}
