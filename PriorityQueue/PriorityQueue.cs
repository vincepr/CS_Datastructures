using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS_DATASTRUCTURES.PriorityQueue;

internal static class Example
{
    public static void Run()
    {
        {
            Console.WriteLine("--- Example: MAX PRIORITY QUEUE ---");
            var max = new MaxPriorityQueue<int, char>();

            max.Add(5, 'a');
            max.Add(9, 'b');
            max.Add(12, 'c');
            max.Add(13, 'd');
            max.Add(16, 'e');
            max.Add(45, 'f');

            foreach (var item in max) Console.WriteLine(item);

            Console.WriteLine($"Count: {max.Count}");
            for (int i = max.Count; i > 0; i--)
            {
                //var (_,value) = min.Pop();
                Console.WriteLine("Out: " + max.Pop());
                Console.WriteLine($"new Count: {max.Count}");
                //foreach (var item in min) Console.WriteLine(item);
            }
        }
        {
            Console.WriteLine("--- Example: MIN PRIORITY QUEUE ---");
            var min = new MinPriorityQueue<int, char>();

            min.Add(5, 'a');
            min.Add(9, 'b');
            min.Add(12, 'c');
            min.Add(13, 'd');
            min.Add(16, 'e');
            min.Add(45, 'f');

            foreach (var item in min) Console.WriteLine(item);

            Console.WriteLine($"Count: {min.Count}");
            for (int i = min.Count; i > 0; i--)
            {
                //var (_,value) = min.Pop();
                Console.WriteLine("Out: " + min.Pop());
                Console.WriteLine($"new Count: {min.Count}");
                //foreach (var item in min) Console.WriteLine(item);
            }
        }
       
    }
}


public class MaxPriorityQueue<TPriority, TValue> : IEnumerable<KeyValuePair<TPriority, TValue>>
{
    /// <summary>
    /// Even though we store it as an array-like form it is actually a Binary Tree. 
    /// </summary>
    //           0 == root,
    //          /        \
    //   1 == left     2 == right
    //  /        \        /       \
    // 3 left  4 right  5left    6 right

    private protected List<KeyValuePair<TPriority, TValue>> _heap;

    private protected IComparer<TPriority> _comparer;

    public int Count => _heap.Count;

    public MaxPriorityQueue() : this(Comparer<TPriority>.Default) { }

    public MaxPriorityQueue(IComparer<TPriority> comparer)
    {
        _heap = new List<KeyValuePair<TPriority, TValue>>();
        _comparer = comparer;
    }


    public void Add(TPriority priority, TValue value)
    {
        _heap.Add(new KeyValuePair<TPriority, TValue>(priority, value));
        HeapifyUp(_heap.Count - 1);
    }
    public void Insert(params KeyValuePair<TPriority, TValue>[] values)
    {
        foreach (var value in values) Add(value.Key, value.Value);
    }

    // bring heap back into heap-state after Insert(). does so by swapping with parent till uptop or bigger no more
    private void HeapifyUp(int idx)
    {
        while (Compare(idx, Parent(idx)))
        {
            Swap(Parent(idx), idx);
            idx = Parent(idx);
        }
    }

    public TValue? Peek() => (_heap.Count > 0) ? _heap[0].Value : default;

    // pops the value with highest priority from our Priority queue
    public (TValue? value, bool success) Pop()
    {
        int len = _heap.Count - 1;
        if (len < 0) return (default(TValue), false);

        // swap last element in place of removed first
        var value = _heap[0].Value;
        _heap[0] = _heap[len];
        _heap.RemoveAt(len);
        HeapifyDown(0);
        return (value, true);
    }

    // bring heap back into heap-state after a Pop()
    // does so by potentially swapping with bigger child, moving down till bottom/no more swap
    private void HeapifyDown(int idx)
    {
        int current = idx;
        int last = _heap.Count - 1;
        var (left, right) = (Left(idx), Right(idx));
        while (left <= last) {
            if (left == last)
                current = left;
            else if (Compare(left, right))
                current = left;
            else
                current = right;

            if (_comparer.Compare(_heap[idx].Key, _heap[current].Key) < 0)
            {
                Swap(idx, current);
                idx = current;
                (left, right) = (Left(idx), Right(idx));
            }
            else return;
        }
    }

    // helpers  - to 'translate' from array structure to binary tree representation it represents
    private static int Parent(int idx) => (idx - 1) / 2;

    private static int Left(int idx) => 2 * idx + 1;

    private static int Right(int idx) => 2 * idx + 2;

    private void Swap(int idx1, int idx2)
        => (_heap[idx1], _heap[idx2]) = (_heap[idx2], _heap[idx1]);

    private bool Compare(int idx1, int idx2)
        => _comparer.Compare(_heap[idx1].Key, _heap[idx2].Key) > 0;

    public IEnumerator<KeyValuePair<TPriority, TValue>> GetEnumerator()
        => ((IEnumerable<KeyValuePair<TPriority, TValue>>)_heap).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable)_heap).GetEnumerator();
}

public class MinPriorityQueue<TPriority, TValue> : MaxPriorityQueue<TPriority, TValue>
{
    // we just have to map -1 -> +1 && +1 -> -1 && 0 -> 0
    // and we changed our MaxPriorityQueue to a MinPriorityQueue
    // we do this by wrapping the Comparer and negating all comparisons.

    private class InverseComparer : IComparer<TPriority>
    {
        private Comparer<TPriority> _originalComparer;

        public InverseComparer(Comparer<TPriority> comparer)
        {
            this._originalComparer = comparer;
        }

        public int Compare(TPriority? x, TPriority? y)
        {
            return -_originalComparer.Compare(x, y);
        }
    }

    public MinPriorityQueue() : base(new InverseComparer(Comparer<TPriority>.Default)) { }

    public MinPriorityQueue(IComparer<TPriority> comparer)
    {
        _heap = new List<KeyValuePair<TPriority, TValue>>();
        _comparer = new InverseComparer((Comparer<TPriority>)comparer);
    }
}
