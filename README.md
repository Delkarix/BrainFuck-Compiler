# BrainFuck-Compiler
A compiler that compiles BrainFuck into an EXE file.

Usage: BrainF FILE

Compiles the file into an EXE. To preserve file size and dynamically compile the file, the file is compiled with the .NET Framework 4.7.1 `System.CodeDom` and `System.CodeDom.Compiler` namespaces. I did this so I would not have to hard-code another compiler like `g++`, `gcc`, or `mono` into the program. It was an awful pain to make this since the CodeDom can be very picky but also extremely variadic at the same time. (Not to mention the fact that not all of .NET's syntax could be expressed through simple classes and delegates, yet a lot of the lesser-used features were expressed multiple times.) If you are running Linux, you must use `mono` to run both the compiler and the files it generates.

I am also planning to use similar compilation methods when working with my own programming language, Delkarix (documentations for Delkarix can be found on the repository in my profile).

I also made a BrainFuck Injector which can also be found in a repository on my profile.
