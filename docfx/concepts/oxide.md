# Getting Started with Oxide

## History

Oxide is a utility library providing some helpful APIs that I've come across
over the years. 

Its original inspiration was me playing with Rust's [option types](options.md)
and [result types](results.md).

The ["smart" parsing](parse.md) code came later, as an exploration in faking
trait-like behavior; its initial implementation lacks trait-like behavior due to
Oxide targeting `netstandard1.3`, which lacked significant chunks of the
Reflection APIs needed to search for `Parse`/`TryParse` implementations. Oxide
1.0 introduced this behavior for the first time, as part of retargeting
everything to `netstandard2.0`.

The object & enumerable extensions came later, via suggestions from my friend
[Ed](https://twitter.com/edropple) for the Kotlin-inspired extensions, and APIs
I've implemented over-and-over in projects for the rest.

## Installation

Oxide is distributed via NuGet. The core package is available at
http://nuget.org/packages/Oxide, and the HTTP extensions are at
https://nuget.org/packages/Oxide.Http. It's a pure .NET Standard 2.0 assembly,
which means it should be usable everywhere—likely, Unity is supported as well,
but is not tested.

## Getting Started

Check out the other conceptual documentation pages for high-level descriptions
of concepts, and check out the [samples](../samples/samples.md) for actual usage examples,
covering all of the various provided APIs. 