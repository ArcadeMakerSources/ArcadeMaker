# Here are some of the main things left to do in the project:

## Core Engine

* Implement as many common built-in functions as possible, such as `directionToPoint(...)`, `setCursor(...)`, `enterFullScreen()`, `positionFree(...)`, etc.

## IDE

* Add tabs to the code editor. Then, make double-clicking an error in the error list open the corresponding document in a new tab in the existing code editor, or in a new code editor window if none is open.
* Add auto-completion for known function and property names, along with a parameter hint (argument list) popup.
* Finish the image editor (this one's going to take a while...).

## Platforms

* Add a **Publish Game** button that can generate `.exe`, `.apk`, `.html`, or other target formats depending on the selected platform.
* Add mobile support. MonoGame already supports mobile, but ArcadeMaker's current functionality does not.
* Add HTML5 support using KNI Engine (Google it...).

## Language

* Improve the syntax for class definitions.
* Fix the bug where using `if`, `else`, `while`, `for`, or `foreach` without `{}` sometimes causes unexpected behavior. (Until this is fixed, **ALWAYS USE BRACKETS `{}`!**)
* Implement math and I/O libraries.
