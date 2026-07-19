# ArcadeMaker

ArcadeMaker is a simple 2D cross‑platform game engine that includes its own programming language (its prototype is called Exp) and an integrated IDE.  
The engine and IDE themselves are written in C#, but the language you use inside ArcadeMaker to program your games is my custom language, not C#.

Currently, the graphics and audio backend is powered by MonoGame, which allows exporting to desktop, mobile, and consoles. I’m also planning to add a KNI-engine implementation to enable web support.

This project is far from finished — not even close.  
But since I have very little time to work on it, I decided to open‑source it in the hope that others will find it interesting and help turn it into something real.

ArcadeMaker is based on GameMaker 8, so if you’ve ever used it, you’ll find it very easy to learn.

---

# Project Background

ArcadeMaker has been built in several stages over the span of a few years, and different parts of it were written at very different points in my programming journey.

- The IDE was written about two years ago, when my programming skills were very different from what they are today.  
- The Exp language was created after the IDE, but still before the current core engine and before the MonoGame implementation existed.

Originally, ArcadeMaker started as a C#‑based engine where users wrote their game logic directly in C#. After leaving the project for a while, I eventually created my own programming language and decided to return to ArcadeMaker — replacing the old C# scripting layer with my language and rewriting the core engine around it.

Because of this long, staggered development history, the IDE currently includes many features that the new engine backend does not yet support. For example:

- Object events — only Create, Step, and Draw events are currently functional.  
- Parent objects — supported in the IDE, not yet implemented in the engine.  
- View following speed — views can follow an object, but the view’s movement speed always matches the object’s speed, even if you set a custom follow speed. This should be straightforward to fix.

The long‑term goal is to bring the new engine up to feature parity with the old C# version — and then expand far beyond it.

---

# Planned Features

- Full implementation of the missing IDE features in the engine  
- KNI-engine backend for web export  
- A more complete and stable version of the Exp language  
- Auto‑completion for the code editor  
- Implementing all the common, daily‑use game functions such as drawRect(), setFont(), deactivateInstancesInRegion(), setRoomWidth(), and many more  
- Cross‑platform templates for mobile and console builds  
- Documentation, tutorials, and example projects

---

# About the GitHub Release

The build available in the Releases section is not a real release. It’s extremely early, contains many bugs, and is missing major features. There is no “Create Standalone Executable” option for exporting finished games — and realistically, you won’t be able to finish a full game with the current state of the engine anyway.

I uploaded this build only so you can see what the project currently looks like and get a sense of its potential. It’s a snapshot of the engine and IDE as they are right now, not a stable or complete version.

---

# Why Open Source?

I love working on ArcadeMaker, but I don’t have enough time to develop it at the pace it deserves.  
By open‑sourcing it, I hope others will:

- Experiment with the engine  
- Improve the language and tooling  
- Add missing features  
- Fix bugs  
- Help shape the future of the project  

If you enjoy GameMaker‑style workflows or want to contribute to a lightweight, beginner‑friendly 2D engine, you’re more than welcome to join.

---

# How to Build

1. Clone the repository.  
2. Open the solution in your preferred C# IDE (Visual Studio, Rider, or VS Code).  
3. Restore NuGet packages.  
4. Build the project.  
5. Run the IDE project to start ArcadeMaker.

---

# Contributing

Contributions of all kinds are appreciated:

- Code improvements  
- Bug fixes  
- Documentation  
- Feature proposals  
- Engine backend implementations  
- Language design ideas  

Before submitting a pull request, please open an issue to discuss your idea.

---

# License

This project is licensed under the MIT License.  
See the LICENSE file for details.

---

# Note About This README and AI Usage

I used GPT to help write this README file.

And since AI is mentioned, a few words about it:  
This project contains almost 0% AI‑generated code or architecture. The only exception is that I used Claude to write the Separating Axis Theorem formulas for collision detection, because that math is a bit too heavy for me. Other than that, everything — the engine, the IDE, the language, the architecture — is hand‑written.

Not because there’s anything wrong with using AI as a programming tool. I know it’s a huge part of the future of software development.  
It’s just that I built this project for fun and for the challenge, and personally I enjoy writing the code myself rather than having a machine do it for me.

In the era of AI and its growing power, I still believe a complete version of this project could be genuinely useful. ArcadeMaker is a beginner‑friendly engine, and people will always need to understand how code works, even if they eventually stop writing every line by hand. The only real way to learn that is by actually writing code.  
And beyond learning, this engine can also be a great tool for anyone who simply wants to make games for fun, by themselves, without relying on AI to do all the work.
