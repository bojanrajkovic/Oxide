# "Smart" Parsing

## Inspiration

Smart parsing is, at its core, inspired by the idea of traits.

Traits are, at their core, _extension interface_. That is, they consist of an
interface definition (in Rust, they are defined against some unknown type
`Self`), and some implementation of the trait for a type `T`. Consider the
following Rust example, where `Foo` is actually defined in some other
crate/package entirely:

```rust
struct SomeErrorType { ... }

trait Parseable {
    fn parse(src: &'static str, ...) -> Self;
    fn try_parse(src: &'static str, ...) -> Result<Self, SomeErrorType>;
}

impl Parseable for Foo {
    fn parse(src: &'static str, ...) -> Foo {
        return Foo::parse(src);
    }

    fn try_parse(src: &'static str, ...) -> Result<Foo, SomeErrorType> {
        let fooParse = Foo::try_parse(src);
        return match fooParse {
            Ok(foo) => foo;
            Err(error) => SomeErrorType::from_foo_error(error)
        };   
    }
}
```

This is entirely valid for a Rust trait. The crate providing the Parseable trait
can provide implementations for types in other crates&mdash;for example, you
want to provide parsing helpers for things in the `std` crate (Rust's standard
library).

The same thing might look like this in C#:

```csharp
class SomeErrorType : Exception { ... }

trait interface IParseable<T> {
    static T Parse(string src, ...);
    static Result<T, SomeErrorType> TryParse(string src, ...);
}

trait class IParseable<Foo> {
    static Foo Parse(string src, ...) {
        return Foo.Parse(src, ...);
    }

    static Result<Foo, SomeErrorType> TryParse(string src, ...) {
        // Notice this example calls the exception-throwing Parse method
        // in order to get useful error output. :( This is because TryParse
        // with a useful error via the Result type is not a pattern in .NET.
        try {
            return Foo.Parse(src, ...);
        } catch (Exception e) {
            return new SomeErrorType(e);
        }
    }
}

// Elsewhere...

public static class StringExtensions
{
    public static T Parse<T>(this string src, params object[] args) {
        // Implementation elided, but can check if `T` implements `IParseable<T>`
        // or if it is provided by a trait, and call `Parse` via the interface.
        …
    }
}
```

Note that this is syntax I made up on the fly&mdash;it should not be taken as an
endorsement of any kind, only wishful thinking.

With the trait in place, you can call `"someString".TryParse<Foo>(…)` as if it
were a regular method, even if `Foo` did not define `Parse` or `TryParse`. The
restrictions follow the same pattern as extension methods, but the method
appears on the type parameter `T`.

The "smart" parsing implementation in Oxide allows a makeshift version of this.
It adds `Parse<T>` and `TryParse<T>` methods as extension methods on `string`
(as the above example does), and uses reflection to search for `Parse` and
`TryParse` methods. First, the given type `T` is searched (in the case of
standard library types that provide those methods), and then, all loaded
assemblies are searched for extension methods.

This is not very performant the first time any type `T` is used, as you might
imagine. Subsequent invocations are fast due to caching of the resulting
generated delegate.

This is not intended to be a seriously used API, and is likely to be removed in
Oxide 2.0. In the meantime, it was fun writing it!