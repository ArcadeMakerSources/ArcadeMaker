# Exp Language: Overview
The language used by this engine for scripting is a custom language whose prototype is called Exp. It is interpreted by C#, using the .NET GC as its garbage collector.  
The language is inspired by many others, including JS, C++, and of course C#.  
It is dynamically typed, even though I’m usually against this kind of programming. However, I decided to make it this way because I wanted the language to be beginner‑friendly, and I have to admit that dynamic typing removes a lot of complexity.

Not because writing the variable type is complex or less readable—I don’t think it is—but because with a dynamically typed architecture you don’t need interfaces, generics, or many other similar constructs.

Local variables
In Exp, you declare a local variable using the `var` or `const` keywords:
```
var x = 0
x = 1 // valid
const y = 0
y = 1 // error
```

Functions
A function is declared using the `func` keyword:
```
func multiply(a, b)
{
    return a * b
}
```
By default, parameters are expected to receive a non-null argument. If you pass `null` as one of the arguments to the function above, a runtime exception will be thrown, stating that the parameter does not accept null.

To declare a nullable parameter, add a `?` suffix to the parameter name:
```
func createUser(name, password, passwordHint?)
{
   // "name" and "password" are guaranteed to be non-null, but "passwordHint" may be null
   ...
}
```

If your function body contains only a single short return statement, you can use the => operator followed by the expression instead of { return ... }:
func add(a, b) => a + b

## Classes
Unlike many other dynamically typed languages, in Exp all class properties must be declared along with the class, and you cannot add new properties to an existing object instance at runtime.

Here’s a simple class declaration:
```
class Product (/properties:/ name, salary)
{
    constructor (name, salary)
    {
        // initialize the properties
        this.name = name // I have to use "this." because the "name" parameter hides the "name" property
        this.salary = salary
    }

    func print()
    {
        println(name + " (" + salary + "$)")
    }
}
```

Classes can also contain static members, which are associated with the class itself (not with a specific instance) and do not require an instance reference:
```
class Point (x, y)
{
    static const zero = new Point(0, 0) // static properties are declared inside the class body
    ...
    static func fromCurrLocation()
    {
       return new Point(GPS.getCurrLat(), GPS.getCurrLong())
    }
}

// using static members:
var pnt = Point.zero
const loc = Point.fromCurrLocation()
```

## Code organization
Exp uses namespaces for code organization. You define the namespace of a document using the namespace keyword at the beginning:
```
namespace game:
// all classes / functions / attributes declared in this document belong to the "game" namespace
```

To access a definition from another namespace, you can use the `::` symbol to separate the namespace from the definition, or mark the entire document as using that namespace:
```
// option 1:
namespace game:
println(math::sqrt(16))

// option 2:
using math
namespace game:
println(sqrt(16))
```

In ArcadeMaker, every script document added to an object event automatically uses the Exp standard namespace (system) and the ArcadeMaker engine namespace (ArcadeMaker) by default, so you don’t need to write using directives for them each time.
