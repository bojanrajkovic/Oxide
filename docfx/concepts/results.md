# Result Types

## Inspiration

Result types in Oxide are an implementation of what is usually called the Either monad.

However, as in the case of [option types][^options], `Result<T, E>` is inspired by [Rust's `Result<T, E>`][^rust] API, due to its ergonomics and humanistic terminology.

APIs should return `Result<T, E>` whenever errors are recoverable, or error propagation is desired. Rust uses them primarily for I/O in the `std` crate, but they are richly integrated into the ecosystem.

The primary approach to consuming them in Rust is via pattern matching. Unfortunately, C#'s pattern matching is not quite as strong, but additional API provided by Oxide's Result types means that we can use it anyway, with some help from the `when` clause.

As usual, see the [samples][^samples] for demo code.

## Why Result Types

Result types provide for natural chaining of operations, where any operation in the chain can fail, while preserving the original error at the end. Scott Wlaschin of F# for Fun And Profit refers to this as [railway oriented programming][^railway].

Result types allow this for both synchronous operations, as well as asynchronous ones, including handy helpers for sanely chaining `Task<Result<T, E>>` without the syntactic horror of nesting `await` calls.

[^railway]: https://fsharpforfunandprofit.com/rop/
[^options]: options.md
[^rust]: https://doc.rust-lang.org/stable/std/result/index.html
[^samples]: ../samples/samples.md