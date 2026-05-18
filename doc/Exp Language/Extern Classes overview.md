# Extern Classes

Extern classes provide an interface for interacting with C# APIs from within Exp code.

To define an extern class, use:

`
extern class ClassName = "C#FullTypeName"
`

Example:

`
extern class File = "System.IO.File"
`

For generic types, use the reflection-style name, such as:
"List`1[System.String]"

---

## Using Extern Classes

Once an extern class is defined, you can:

- Invoke its static methods or get/set its static properties.
- Use the new keyword to create an instance and access its non‑static members.

If an extern method returns an instance of a class you did not explicitly define as extern, you can still use that instance normally.

You can also use the is operator on extern instances.

---

Example: Using `System.Text.StringBuilder`

```
extern class StringBuilder = "System.Text.StringBuilder"

func makeGreeting(name) {
    const sb = new StringBuilder()
    sb.append("Hello, ")
    sb.append(name)
    sb.append("!")
    return sb.toString()
}

const message = makeGreeting("Ben")
println(message) // prints: Hello, Ben!
```

Notes:

- StringBuilder is a real .NET class with a constructor and instance methods.
- Exp automatically lowercases method/property names.
- Strings and numbers are converted to their C# equivalents.

---

Example: Using System.IO.File

```
extern class File = "System.IO.File"

func readConfig() {
    const content = File.readAllText("config.txt")
    return content
}

func writeLog(message) {
    File.appendAllText("app.log", message + "\n")
}

const config = readConfig()
print(config)

writeLog("Application started")
```

Notes:

- System.IO.File is a static class, so its methods are called directly.
- Strings passed to File methods are converted to C# strings.
- Demonstrates reading and writing files.

---
