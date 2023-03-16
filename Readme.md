# DynamicReflector

**DynamicReflector** is a C# library that provides an easy way to access fields and 
methods on objects and static classes using dynamic and reflection.

Powered by [Traverse](https://github.com/pardeike/Harmony/blob/master/Harmony/Tools/Traverse.cs) 
from [Harmony](https://github.com/pardeike/Harmony) 
by [Andreas Pardeike](https://github.com/pardeike).

## How it works
The Reflector object binds to another object and uses reflection to access its fields 
and methods.

When you access a member of the `Reflector` dynamic object, a new Reflector object is 
returned that is bound to the resulting object.

```chsarp
// dynamic reflector
dynamic fieldReflection = reflector.filed;
dynamic methodReflection =  reflector.method();
```

When you write to a member of the `Reflector` dynamic object, the value is written 
directly to the bound object.

```chsarp
// dynamic reflector
reflector.filed = 7; // changes binded object filed value
```

## Installation
Install [nuget packet](link)

via dotnet
```
dotnet add package DynamicReflector
```

via nuget
```
Install-Package DynamicReflector
```

## Usage

```csharp
// Getting reflector for an object
object obj = new InternalClass();
dynamic reflector = obj.ToReflector();

// ... or using new()
dynamic reflectorNew = new Reflector(obj);

// ... or using Create()
dynamic reflectorCreate = Reflector.Create(typeof(InternalClass), 1337);

// Getting private field value
// We need to explisitly cast OriginalObject to desired type
var value = (int)reflector.value.OriginalObject;

// Setting private field value
reflector.value = 7; // changes obj.value

// Call a method and get the result
var result = (int)reflector.ReturnValue().OriginalObject;

// Creating reflector for a class to call static methods
dynamic staticReflector = Reflector.CreateStatic(typeof(InternalClass));

// Call static method
var staticMethodResult = staticReflector.StaticMethod().OriginalObject;
```

Where `InternalClass` is:
```csharp
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
```