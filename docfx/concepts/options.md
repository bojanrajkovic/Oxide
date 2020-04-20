# Option Types

## Inspiration

Option types like Oxide's are an implementation of what is usually called the
Maybe monad.

However, in Oxide's case, the implementations are inspired entirely by Rust's
API, due to its ergonomics. Many other implementations exist, but they tend to
hew too closely to the monadic terminology from the Haskell/ML language
families. However, this terminology is heavy on mathematical symbols and jargon,
and can lack accessibility. 

Meanwhile, Rust's implementation and API takes a more human approach to API
naming—for example, what Oxide calls `AndThen` (`and_then` in Rust), is
frequently called `flatMap`. The former is a relatively reasonable name—it's
indicative of what the implementation behavior is. `flatMap` is a more
theoretical term for the operation, but reveals relatively little about its
behavior unless you're already well-versed in the material.

## Why Option Types?

Option types are common in Rust code, because of their broad spectrum of uses
(list borrowed from [the Rust docs][^rust]):

* Initial values
* Return values for functions that are not defined over their entire input range
  (partial functions)
* Return value for otherwise reporting simple errors, where None is returned on
  error
* Optional struct fields
* Struct fields that can be loaned or "taken"
* Optional function arguments
* Nullable pointers
* Swapping things out of difficult situations

Most of these are relevant to the .NET space as well, and we'll hopefully cover
them in our [samples][^samples]. However, it is clear that option types can help us reduce
or eliminate certain types of bugs in our code, as well as aid in the
construction of more composable application logic. 

[^rust]: https://doc.rust-lang.org/stable/std/option/index.html
[^samples]: ../samples/samples.md