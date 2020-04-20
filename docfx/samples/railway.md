---
title: Railway-Oriented Programming
---

A few years ago, Scott Wlaschin introduced a very handy explanation for the
pattern, already common in functional languages, of chaining implementations of
the `Either` ~~burrito~~ monad. He called it ["railway-oriented
programming,"][rop] and I've been a fan of both the concept and the name ever
since. I'm not going to cover here what a ~~burrito~~ monad is: the reading list
in Scott's post/talk does a great job of that already.

Oxide's [`Result<T, E>`][result], being an implementation of the `Either`
~~burrito~~ monad, enables straightforward railway programming for **synchronous**
scenarios:

```csharp
using Oxide;

class Order { ... }
class OrderResult { ... }
class Error { ... }

Result<Order, Error> ValidateOrder(Order o) { ... }
Result<Order, Error> UpdateOrderDatabase(Order o) { ... }
Result<OrderResult, Error> SendEmail(Order o) { ... }

Order o = ...;
var result = ValidateOrder(o).AndThen(UpdateOrderDatabase).AndThen(SendEmail);
```

Trying to use this with asynchronous code, you quickly run into a soup of
`await`, and parentheses, and general spaghetti code:

```csharp
using Oxide;

class Order { ... }
class OrderResult { ... }
class Error { ... }

async Task<Result<Order, Error>> ValidateOrderAsync(Order o) { ... }
async Task<Result<Order, Error>> UpdateOrderDatabaseAsync(Order o) { ... }
async Task<Result<OrderResult, Error>> SendEmailAsync(Order o) { ... }

await (
    await (
        await ValidateOrderAsync(...)
    ).AndThenAsync(UpdateOrderDatabaseAsync)
).AndThenAsync(SendEmailAsync);
```

This is hard to read and hard to follow. You can clarify the chain itself by
splitting it into separate statements:

```csharp
var first  = await ValidateOrderAsync(...);
var second = await first.AndThenAsync(UpdateOrderDatabaseAsync);
var third = await second.AndThenAsync(SendEmailAsync);
```

You still end up with repeated `await` and extra variables. With
[`AndThenAsync<TIn, TOut, TError>`][and-then-async], you can chain async
`Result<T, E>` calls without causing a lot of spaghetti:

```csharp
var result = await ValidateOrderAsync(...)
    .AndThenAsync(UpdateOrderDatabaseAsync)
    .AndThenAsync(SendEmailAsync);
```

You gain all the benefits of railway-oriented programming, as well as clean,
readable code, even when dealing with asynchronous calls. With the other
overload of [`AndThenAsync<TIn, TOut, TError>`][sync-overload], you can even
chain synchronous steps in with asynchronous steps:

```csharp
Result<Order, Error> ComputeDiscounts(Order o) { ... }

var result = await ValidateOrderAsync(...)
    .AndThenAsync(ComputeDiscounts)
    .AndThenAsync(UpdateOrderDatabaseAsync)
    .AndThenAsync(SendEmailAsync);
```

[rop]: https://fsharpforfunandprofit.com/rop/
[result]: /api/Oxide.Result-2.html
[and-then-async]: /api/Oxide.Results.html#Oxide_Results_AndThenAsync__3_System_Threading_Tasks_Task_Oxide_Result___0___2___System_Func___0_System_Threading_Tasks_Task_Oxide_Result___1___2____
[sync-overload]: /api/Oxide.Results.html#Oxide_Results_AndThenAsync__3_System_Threading_Tasks_Task_Oxide_Result___0___2___System_Func___0_Oxide_Result___1___2___