# Oxide

A collection of Rust-like types for .NET.

Build: [![Build status](https://ci.appveyor.com/api/projects/status/434eaimi9rdft2fn?svg=true)](https://ci.appveyor.com/project/bojanrajkovic/oxide)  
Coverage: [![Coverage Status](https://coveralls.io/repos/github/bojanrajkovic/Oxide/badge.svg?branch=master)](https://coveralls.io/github/bojanrajkovic/Oxide?branch=master)

## Implemented Types

* [std::option::Option][rust-option]: [Option.cs][our-option]
* [std::result::Result][rust-result]: [Result.cs][our-result]

## Building

1. Clone this repo.
2. Run `dotnet build` from the root, or use Visual Studio/VS Code.

## Running Tests

1. Run `dotnet test` in `src\Oxide.Tests` or use any other xUnit.NET + .NET
   Core-compatible test runner.

[rust-option]: https://doc.rust-lang.org/std/option/enum.Option.html
[our-option]: src/Oxide/Option.cs
[rust-result]: https://doc.rust-lang.org/std/result/enum.Result.html
[our-result]: src/Oxide/Result.cs
