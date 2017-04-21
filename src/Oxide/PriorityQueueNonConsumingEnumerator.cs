using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Oxide
{
    public partial class PriorityQueue<TPriority, TItem> : IEnumerable<TItem>, IOrderedEnumerable<TItem>
    {
        class PriorityQueueNonConsumingEnumerator : IEnumerator<TItem>
        {
            PriorityQueue<TPriority, TItem> queue;
            int version;

            int keyIndex;
            IReadOnlyCollection<TPriority> keys;
            LinkedList<TItem> currentQueue;
            LinkedListNode<TItem> currentItem;

            public PriorityQueueNonConsumingEnumerator(PriorityQueue<TPriority, TItem> queue) {
                keys = queue.buckets.Keys as IReadOnlyCollection<TPriority>;
                this.queue = queue;
                this.version = queue.nonConsumingVersion;

                Reset();
            }

            void UpdateKeyIndex()
            {
                switch (queue.QueueType) {
                    case QueueType.Min:
                        keyIndex++;
                        break;
                    case QueueType.Max:
                        keyIndex--;
                        break;
                }
            }

            void UpdateCurrentQueue()
            {
                while (true) {
                    // Advance the key index.
                    UpdateKeyIndex();

                    // If we're out of range, end.
                    if (keyIndex < 0 || keyIndex > (queue.buckets.Keys.Count - 1)) {
                        currentQueue = null;
                        return;
                    }

                    // If the current queue is non-empty, we're good.
                    currentQueue = queue.buckets[keys.ElementAt(keyIndex)];
                    if (currentQueue.Count > 0)
                        break;
                }
            }

            public TItem Current => currentItem.Value;

            object IEnumerator.Current => Current;

            public void Dispose () {}

            public bool MoveNext()
            {
                if (version != queue.nonConsumingVersion)
                    throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");

                if (currentQueue == null)
                    return false;

                // If the current item is null or doesn't belong to the current queue, start from the current queue's
                // first item. We know the queue must have at least one item because our filter filters out 0-item
                // queues.
                if (currentItem == null || !object.ReferenceEquals(currentItem.List, currentQueue)) {
                    currentItem = currentQueue.First;
                    return true;
                } else {
                    // If there's a next item, set it, return true.
                    if (currentItem.Next != null) {
                        currentItem = currentItem.Next;
                        return true;
                    }

                    // If there's no next item, update the queue.
                    UpdateCurrentQueue();

                    // If there is no new queue, return false.
                    if (currentQueue == null)
                        return false;

                    // Otherwise, set the new queue's first item as the current item and return true.
                    currentItem = currentQueue.First;
                    return true;
                }
            }

            public void Reset()
            {
                keyIndex = queue.QueueType == QueueType.Max ? keys.Count: -1;
                UpdateCurrentQueue();
            }
        }
    }
}