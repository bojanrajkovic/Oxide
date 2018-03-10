using Xunit;

using Oxide;

namespace Oxide.Tests
{
    public class EnumerableTests
    {
        static readonly string[] sequence = {
            "rose", "violet", "daisy", "buttercup" 
        };

        [Fact]
        public void Calling_tail_returns_remainder_of_sequence() =>
            Assert.Equal(new [] {
                "violet",
                "daisy",
                "buttercup"
            }, sequence.Tail());

        [Fact]
        public void Calling_head_is_equivalent_to_first() =>
            Assert.Equal("rose", sequence.Head());

        [Fact]
        public void Calling_rest_is_equivalent_to_tail() =>
            Assert.Equal(sequence.Tail(), sequence.Rest());
    }
}