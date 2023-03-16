using DynamicReflector.Extensions;

namespace DynamicReflector.Samples
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Getting reflector for an object
            object obj = new InternalClass();
            dynamic reflector = obj.ToReflector();

            // ... or using new()
            dynamic reflectorNew = new Reflector(obj);

            // ... or using Create()
            dynamic reflectorCreate = Reflector.Create(typeof(InternalClass), 1337);

            // Getting private field value
            // need to explisitly cast OriginalObject to desired type
            var value = (int)reflector.value.OriginalObject;

            // Setting private field value
            reflector.value = 7; // changes obj.value

            // Call a method and get the result
            var result = (int)reflector.ReturnValue().OriginalObject;

            // Creating reflector for a class to call static methods
            dynamic staticReflector = Reflector.CreateStatic(typeof(InternalClass));

            // Call static method
            var staticMethodResult = staticReflector.StaticMethod().OriginalObject;
        }
    }
}