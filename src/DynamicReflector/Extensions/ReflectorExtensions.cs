namespace DynamicReflector.Extensions
{
    public static class ReflectorExtensions
    {
        public static Reflector ToReflector(this object obj)
        {
            return new Reflector(obj);
        }
    }
}
