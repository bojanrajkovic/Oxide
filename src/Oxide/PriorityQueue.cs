using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using static Oxide.Options;

namespace Oxide
{
    public partial class PriorityQueue<TPriority, TItem> : IEnumerable<TItem>, IOrderedEnumerable<TItem>
    {
        // We'll have a thread-safe queue via locking.
        static readonly object syncRoot = new object ();

        // Use this to check our enumerator. nonConsumingVersion is checked
        // by the non-consuming enumerator and is incremented by pushes and pops.
        // consumingVersion is checked by the consuming iterator and is only
        // incremented by pushes.
        int nonConsumingVersion = 0, consumingVersion = 0;

        // Keep a queue per priority, using a sorted dictionary.
        SortedDictionary<TPriority, LinkedList<TItem>> buckets;

        // Priority comparer.
        IComparer<TPriority> comparer;
        Func<TItem, TPriority> selector;

        public QueueType QueueType { get; }
        public bool IsEmpty => buckets.Count == 0 || buckets.Sum(k => k.Value.Count) == 0;
        public int Count => buckets.Sum(k => k.Value.Count);

        // TODO: Figure out how to represent a constructor where TPriority and TItem are the same
        // type and are IComparable.

        public PriorityQueue(QueueType type, Func<TItem, TPriority> selector) : this(type, null, selector) {}

        public PriorityQueue(QueueType type, IComparer<TPriority> comparer, Func<TItem, TPriority> selector)
            : this(type, null, selector, new TItem[0]) {}

        public PriorityQueue(
            QueueType type,
            IComparer<TPriority> comparer,
            Func<TItem, TPriority> selector,
            IEnumerable<TItem> items
        )
        {
            QueueType = type;
            this.comparer = comparer ?? Comparer<TPriority>.Default;
            this.selector = selector;
            buckets = new SortedDictionary<TPriority, LinkedList<TItem>>(comparer);

            foreach (var item in items)
                Push(item);
        }

        Func<KeyValuePair<TPriority, LinkedList<TItem>>, bool> queueFilter = hp => hp.Value.Count > 0;

        public Option<TItem> Peek()
        {
            if (IsEmpty)
                return None<TItem>();

            var queue = (QueueType == QueueType.Min ? buckets.First(queueFilter) : buckets.Last(queueFilter)).Value;
            return queue.Count == 0 ? None<TItem>() : Some(queue.First.Value);
        }

        public Option<TItem> Pop()
        {
            TItem PopLinkedList(LinkedList<TItem> ll)
            {
                var first = ll.First.Value;
                ll.RemoveFirst();
                return first;
            }

            if (IsEmpty)
                return None<TItem>();

            var queue = (QueueType == QueueType.Min ? buckets.First(queueFilter) : buckets.Last(queueFilter)).Value;
            var res = queue.Count == 0 ? None<TItem>() : Some(PopLinkedList(queue));

            // If we actually popped something off, bump the version, but only the non-consuming version.
            // See note above at `nonConsumingVersion` declaration.
            if (res.IsSome)
                Interlocked.Increment(ref nonConsumingVersion);

            return res;
        }

        public void Push(TItem item)
        {
            lock (syncRoot) {
                Interlocked.Increment(ref nonConsumingVersion);
                Interlocked.Increment(ref consumingVersion);

                var priority = selector(item);
                LinkedList<TItem> queue;

                if (!buckets.TryGetValue(priority, out queue))
                    buckets.Add(priority, queue = new LinkedList<TItem>());

                queue.AddLast(item);
            }
        }

        public void Clear()
        {
            lock (syncRoot) {
                buckets.Clear();
                Interlocked.Increment(ref nonConsumingVersion);
                // Increment the consuming version here too, because clearing the collection
                // is distinct from the enumerator popping everything off.
                Interlocked.Increment(ref consumingVersion);
            }
        }

        // Priority doesn't matter.
        public void Append<TOtherPriority>(PriorityQueue<TOtherPriority, TItem> queue)
        {
            // Don't consume the other queue.
            foreach (var queueItem in queue.buckets.SelectMany(hp => hp.Value))
                Push(queueItem);
        }

        public IOrderedEnumerable<TItem> CreateOrderedEnumerable<TKey>(
            Func<TItem, TKey> keySelector,
            IComparer<TKey> comparer,
            bool descending) => new PriorityQueue<TKey, TItem>(
                descending ? QueueType.Max : QueueType.Min,
                comparer,
                keySelector,
                buckets.SelectMany(queue => queue.Value)
            );

        public IEnumerator<TItem> Consume() => new PriorityQueueConsumingEnumerator(this);

        public IEnumerator<TItem> GetEnumerator() => new PriorityQueueNonConsumingEnumerator(this);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}