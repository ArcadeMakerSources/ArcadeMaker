# `foreach` loops
As any other loop type in Exp, `foreach` loops support the use of `counter` keyword, which lets you declare a special constant that holds the index of the current item in the collection:

`foreach x in items : counter i { println(i + ": " + x) }`

# How to define a class as iterable via `foreach` loops
There are 2 methods for that: 
- if your class has an array property and you just want the `foreach` to iterate over it, add the `basearray` keyword after the property declaration and you're done:
  `class Stack(arr basearray) {...}`
- if your class implements a different method of collection, mark it with the `system::Iteratable` attribute and define these 3 functions:
  `it_hasNext()`, `it_next()`, `it_reset()`
  when `it_hasNext()` is called before each iterate and should return a boolean indicating whether there are more items to iterate on them,
  `it_next()` should return the next item in the collection,
  and `it_reset()` is called when entering the `foreach` loop.
  Example:

  ```
  using system
  namespace specials:
  
  class Node(value, next) {}

  @Iteratable
  class LinkedList(firstNode, currNode)
  {
     ...
     
     func it_hasNext() => currNode != null
     func it_next()
     {
        const item = currNode.value
        currNode = currNode.next
        return item
     }
  
     func it_reset()
     {
        currNode = firstNode
     }
  }
  ```
