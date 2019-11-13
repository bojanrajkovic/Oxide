# Oxide

A library of useful classes and extensions to .NET. Partially based on
implementing types from Rust and Kotlin, partially on useful things I've needed
over and over again on projects.

[![Build Status][ci-image]][ci]

## Things contained

* [std::option::Option from Rust][rust-option]
* [std::result::Result from Rust][rust-result]
* Environment variable helper that uses `Option`
* Async/await expression support for options, contributed by @garuma
* Magical parsing extensions that return Result/Option
* A priority queue, with API inspired by Rust's BinaryHeap
* Some half-baked HTTP wrappers that need a bit of improvement
* `Head`, `Tail`, and `Rest` (same as `Tail`, counterpart to `First`) extensions
  for `IEnumerable<T>`
* Some Kotlin-inspired extension methods like `Let`, `Also`, `Use`, etc. See the
  [Kotlin documentation][kotlin-doc].

## Building

1. Clone this repo.
2. Run `dotnet build` from the root, or use Visual Studio/VS Code.

## Running Tests

1. Run `dotnet test` in `src\Oxide.Tests` or use any other xUnit.NET + .NET
   Core-compatible test runner.

[rust-option]: https://doc.rust-lang.org/std/option/enum.Option.html
[rust-result]: https://doc.rust-lang.org/std/result/enum.Result.html
[ci]: https://github.com/bojanrajkovic/Oxide/actions
[ci-image]: https://github.com/bojanrajkovic/Oxide/workflows/CI/badge.svg
[kotlin-doc]: https://kotlinlang.org/api/latest/jvm/stdlib/kotlin/index.html
