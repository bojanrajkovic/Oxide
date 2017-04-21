using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Oxide
{
    public partial class PriorityQueue<TPriority, TItem> : IEnumerable<TItem>, IOrderedEnumerable<TItem>
    {
        class PriorityQueueConsumingEnumerator : IEnumerator<TItem>
        {
            PriorityQueue<TPriority, TItem> queue;
            int version;

            public PriorityQueueConsumingEnumerator(PriorityQueue<TPriority, TItem> queue) {
                this.queue = queue;
                version = queue.consumingVersion;
            }

            public TItem Current => queue.Pop().Unwrap();

            object IEnumerator.Current => Current;

            public void Dispose() { }

            public bool MoveNext() {
                if (version != queue.consumingVersion)
                    throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
                return queue.Peek().IsSome;
            }

            public void Reset() => throw new InvalidOperationException(
                "This enumerator is a consuming enumerator and cannot be reset."
            );
        }
    }
}