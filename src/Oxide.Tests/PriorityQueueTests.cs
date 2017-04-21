using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Oxide;

using Xunit;

namespace Oxide.Tests
{
    public class PriorityQueueTests
    {
        [Fact]
        public void Peek_does_not_consume_item()
        {
            var heap = new PriorityQueue<int, int>(QueueType.Max, i => i);

            Assert.True(heap.IsEmpty);
            heap.Push(10);
            Assert.Equal(1, heap.Count);

            Assert.Equal(10, heap.Peek());
            Assert.Equal(1, heap.Count);
        }

        [Fact]
        public void Peek_on_empty_heap_returns_none()
        {
            var heap = new PriorityQueue<int, int>(QueueType.Max, i => i);

            Assert.True(heap.IsEmpty);
            Assert.True(heap.Peek().IsNone);
        }

        [Fact]
        public void Pop_consumes_item()
        {
            var heap = new PriorityQueue<int, int>(QueueType.Max, i => i);

            Assert.True(heap.IsEmpty);
            heap.Push(1);
            Assert.Equal(1, heap.Count);

            Assert.Equal(1, heap.Pop());
            Assert.True(heap.IsEmpty);
        }

        [Fact]
        public void Pop_on_empty_heap_returns_none()
        {
            var heap = new PriorityQueue<int, int>(QueueType.Max, i => i);

            Assert.True(heap.IsEmpty);
            Assert.True(heap.Pop().IsNone);
        }

        [Fact]
        public void Push_pushes_items()
        {
            var heap = new PriorityQueue<int, int>(QueueType.Max, i => i);

            heap.Push(11);

            Assert.False(heap.IsEmpty);
            Assert.Equal(1, heap.Count);
        }

        [Theory]
        [InlineData(new [] { 7, 4, 11 }, new [] { 11, 7, 4 }, QueueType.Max)]
        [InlineData(new [] { 7, 4, 11 }, new [] { 4, 7, 11 }, QueueType.Min)]
        public void Items_are_popped_in_correct_order(int[] data, int[] expectedOrder, QueueType type)
        {
            var heap = new PriorityQueue<int, int>(type, i => i);
            foreach (var datum in data)
                heap.Push(datum);

            Assert.Equal(3, heap.Count);

            var popped = new int[expectedOrder.Length];
            for (int i = 0; i < expectedOrder.Length; i++)
                popped[i] = heap.Pop().Unwrap();

            Assert.Equal(expectedOrder, popped);
        }

        [Fact]
        public void Can_clear_heap()
        {
            var heap = new PriorityQueue<int, int>(QueueType.Max, i => i);
            heap.Push(1);
            heap.Push(15);
            heap.Push(8);

            Assert.False(heap.IsEmpty);
            Assert.Equal(3, heap.Count);

            heap.Clear();

            Assert.True(heap.IsEmpty);
            Assert.Equal(0, heap.Count);
        }

        [Fact]
        public void Can_append_binary_heaps()
        {
            var firstHeap = new PriorityQueue<int, string>(QueueType.Max, str => str.Length);
            var secondHeap = new PriorityQueue<string, string>(QueueType.Max, str => str);
            secondHeap.Push("hello");
            secondHeap.Push("taco");
            secondHeap.Push("beantaco");

            Assert.True(firstHeap.IsEmpty);
            Assert.False(secondHeap.IsEmpty);
            Assert.Equal(3, secondHeap.Count);

            firstHeap.Append(secondHeap);

            Assert.False(firstHeap.IsEmpty);
            Assert.False(secondHeap.IsEmpty);
            Assert.Equal(3, firstHeap.Count);
            Assert.Equal(3, secondHeap.Count);

            var expectedStrings = new [] { "beantaco", "hello", "taco" };
            var poppedStrings = new string[3];

            for (int i = 0; i < 3; i++)
                poppedStrings[i] = firstHeap.Pop().Unwrap();

            Assert.Equal(expectedStrings, poppedStrings);
        }

        [Theory]
        [InlineData(new [] { "beans", "taco", "moming" }, new [] { "taco", "beans", "moming" }, false)]
        [InlineData(new [] { "beans", "taco", "moming" }, new [] { "moming", "beans", "taco" }, true)]
        public void Can_create_ordered_enumerable(string[] data, string[] expected, bool descending)
        {
            var firstHeap = new PriorityQueue<string, string>(QueueType.Max, str => str);
            foreach (var datum in data)
                firstHeap.Push(datum);

            Assert.False(firstHeap.IsEmpty);
            Assert.Equal(3, firstHeap.Count);

            var orderedEnumerable = firstHeap.CreateOrderedEnumerable(
                str => str.Length,
                Comparer<int>.Default,
                descending
            );

            var items = orderedEnumerable.ToArray();
            Assert.Equal(expected, items);

            var newHeap = Assert.IsType<PriorityQueue<int, string>>(orderedEnumerable);

            // The newly created heap has not been consumed by the iterator.
            Assert.False(newHeap.IsEmpty);
        }

        [Fact]
        public void Using_consuming_iterator_consumes_queue()
        {
            var bh = new PriorityQueue<int, string>(QueueType.Max, str => str.Length);
            bh.Push("taco");
            bh.Push("cat");
            bh.Push("tacocat");

            var expected = new string[] { "tacocat", "taco", "cat" };
            var enumerated = new List<string>();
            using (var enumerator = bh.Consume()) {
                while (enumerator.MoveNext()) {
                    var str = Assert.IsType<string>(enumerator.Current);
                    enumerated.Add(str);
                }
            }
            Assert.Equal(expected, enumerated);

            // The priority queue has been consumed.
            Assert.True(bh.IsEmpty);
        }

        [Fact]
        public void Consuming_iterator_cannot_be_reset()
        {
            var bh = new PriorityQueue<int, string>(QueueType.Max, str => str.Length);
            var consumer = bh.Consume();
            Assert.Throws<InvalidOperationException>(() => consumer.Reset());
        }

        [Fact]
        public void Trying_to_reset_default_enumerator_does_not_throw_exception()
        {
            var bh = new PriorityQueue<int, string>(QueueType.Max, str => str.Length);
            bh.Push("taco");
            bh.Push("cat");
            bh.Push("tacocat");

            var enumerator = bh.GetEnumerator();

            enumerator.MoveNext();
            enumerator.MoveNext();
            enumerator.MoveNext();

            enumerator.Reset();

            Assert.True(enumerator.MoveNext());
            Assert.Equal("tacocat", enumerator.Current);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_access_via_untyped_enumerator(bool consuming)
        {
            var bh = new PriorityQueue<int, string>(QueueType.Max, str => str.Length);
            bh.Push("taco");
            bh.Push("cat");
            bh.Push("tacocat");

            var ienumerable = (IEnumerable)bh;
            var enumerator = consuming ? bh.Consume() : ienumerable.GetEnumerator();

            var expected = new string[] { "tacocat", "taco", "cat" };
            var enumerated = new List<string>();
            while (enumerator.MoveNext()) {
                var str = Assert.IsType<string>(enumerator.Current);
                enumerated.Add(str);
            }
            Assert.Equal(expected, enumerated);
        }

        [Fact]
        public void Items_with_equal_priority_come_out_in_insertion_order()
        {
            var bh = new PriorityQueue<int, string>(QueueType.Max, str => str.Length);
            bh.Push("tacos");
            bh.Push("beans");

            Assert.False(bh.IsEmpty);
            Assert.Equal(2, bh.Count);

            Assert.Equal("tacos", bh.Pop().Unwrap());
            Assert.Equal("beans", bh.Pop().Unwrap());
        }

        [Fact]
        public void Items_with_equal_priority_come_out_in_insertion_order_via_enumerator()
        {
            var bh = new PriorityQueue<int, string>(QueueType.Max, str => str.Length);
            bh.Push("tacos");
            bh.Push("beans");

            Assert.False(bh.IsEmpty);
            Assert.Equal(2, bh.Count);

            var enumerator = bh.GetEnumerator();

            Assert.True(enumerator.MoveNext());
            Assert.Equal("tacos", enumerator.Current);
            Assert.True(enumerator.MoveNext());
            Assert.Equal("beans", enumerator.Current);
        }

        [Fact]
        public void Modifying_collection_via_addition_causes_enumerator_to_throw()
        {
            var bh = new PriorityQueue<int, string>(QueueType.Max, str => str.Length);
            bh.Push("taco");
            bh.Push("cat");
            bh.Push("tacocat");

            var enumerator = bh.GetEnumerator();

            enumerator.MoveNext();

            bh.Push ("dog");

            var ex = Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());
            Assert.Equal("Collection was modified; enumeration operation may not execute.", ex.Message);
        }

        [Fact]
        public void Modifying_collection_via_removal_causes_enumerator_to_throw()
        {
            var bh = new PriorityQueue<int, string>(QueueType.Max, str => str.Length);
            bh.Push("taco");
            bh.Push("cat");
            bh.Push("tacocat");

            var enumerator = bh.GetEnumerator();

            enumerator.MoveNext();

            bh.Pop();

            var ex = Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());
            Assert.Equal("Collection was modified; enumeration operation may not execute.", ex.Message);
        }

        [Fact]
        public void Modifying_collection_via_addition_causes_consuming_enumerator_to_throw()
        {
            var bh = new PriorityQueue<int, string>(QueueType.Max, str => str.Length);
            bh.Push("taco");
            bh.Push("cat");
            bh.Push("tacocat");

            var enumerator = bh.Consume();

            enumerator.MoveNext();

            bh.Push("dog");

            var ex = Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());
            Assert.Equal("Collection was modified; enumeration operation may not execute.", ex.Message);
        }

        [Fact]
        public void Modifying_collection_via_removal_does_not_throw_with_consuming_enumerator()
        {
            var bh = new PriorityQueue<int, string>(QueueType.Max, str => str.Length);
            bh.Push("taco");
            bh.Push("cat");
            bh.Push("tacocat");

            var enumerator = bh.Consume();

            enumerator.MoveNext();

            bh.Pop();

            enumerator.MoveNext();
        }

        [Fact]
        public void Enumerator_stops_on_empty_collection()
        {
            var bh = new PriorityQueue<int, string>(QueueType.Max, str => str.Length);
            var enumerator = bh.GetEnumerator();
            Assert.False(enumerator.MoveNext());
        }

        [Fact]
        public void Enumerator_skips_empty_queues()
        {
            var bh = new PriorityQueue<int, string>(QueueType.Max, str => str.Length);
            bh.Push("cat");
            bh.Push("doggo");
            bh.Pop();
            var enumerator = bh.GetEnumerator();
            Assert.True(enumerator.MoveNext());
            Assert.Equal("cat", enumerator.Current);
            Assert.False(enumerator.MoveNext());
            Assert.False(enumerator.MoveNext());
        }
    }
}