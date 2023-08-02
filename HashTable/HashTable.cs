using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

/*
 * 
 * Implementation for a Hash Map
 * - using Separate Chaining (with linked list-ish chaining)
 * 
 */

namespace RS_DATASTRUCTURES.HashTable
{
    public static class Example
    {
        public static void Run()
        {
            Console.WriteLine("--- (Simple) HashTable Example: ---");
            var table = new SimpleHashTable<int, int>();

            table.Add(10, 2);    // override
            table.Add(10, 1);
            table.Add(23, 42);
            table.Add(51, 72);
            table.Add(81, 1);
            table.Add(85, 2);
            table.Add(86, 3);  // growth factor reached -> reallocate to 16 capacity here
            foreach (var (key, value) in table)
                Console.WriteLine($"[{key} {value}]");

            Console.WriteLine($"Count: {table.Count} Capacity: {table.Capacity}");

            Console.WriteLine($"{table.Contains(51)} {table.GetValue(51)}");  // true 71
            Console.WriteLine($"{table.Contains(52)} {table.GetValue(52)}");  // false 0    (null-value for int is 0)

            table.Remove(51);
            Console.WriteLine($"{table.Contains(51)} {table.GetValue(51)}");  // true 71

            var table2 = new SimpleHashTable<String, int>(("Bob",24),("James",34));
            foreach(var (key, value) in table2)
                Console.WriteLine($"[{key} {value}]");




        }
    }

    /// <summary>
    /// Implemention of a HashMap using SEPARATE CHAINING with Generic Key-Value pairs.
    /// </summary>
    /// <typeparam name="K">key</typeparam>
    /// <typeparam name="V">value</typeparam>
    public class SimpleHashTable<K, V> : IEnumerable<KeyValuePair<K, V>>
    {
        /// <summary>
        /// Represents a single key-value pair in our list
        /// </summary>
        protected sealed record Entry
        {
            public K key { get; init; }
            public V value { get; set; }
            public int hashcode { get; set; }
            public Entry? next { get; set; }

            public Entry(K key, V value, int hashcode, Entry? next = null)
            {
                this.key = key;
                this.value = value;
                this.hashcode = hashcode;
                this.next = next;
            }
        }

        /* Attributes */
        private const int START_CAPACITY = 8;
        private Entry[] entries;
        public int Count { get; private set; }
        private int version = 0;                      /// version needed to trhow if Enumerator changed while consuming.
        public int Capacity => entries.Length;        /// nr of buckets
        public SimpleHashTable(int CAPACITY = START_CAPACITY)
        {
            CAPACITY = (CAPACITY > START_CAPACITY) ? CAPACITY : START_CAPACITY;
            entries = new Entry[CAPACITY];
        }

        public SimpleHashTable(params (K, V)[] pairs)
        {
            int capacity = (START_CAPACITY > pairs.Length) ? START_CAPACITY : pairs.Length;
            entries = new Entry[START_CAPACITY];
            foreach (var pair in pairs)
                Add(pair.Item1, pair.Item2);
        }

        /* functionality METHODS */
        public void Add(K key, V value)
        {
            version++;
            int hashcode = key.GetHashCode();
            int targetBucket = (hashcode & int.MaxValue) % entries.Length;
            Entry? targetEntry = entries[targetBucket];
            
            if (targetEntry is null)                // add directly to bucket (first element)
            {
                entries[targetBucket] = new Entry(key, value, hashcode, null);
                Count++;
                ReallocateIfNeed();
                return;
            }
            // traverse the linked list:
            while (targetEntry is not null)
            {
                if(key.Equals(targetEntry.key))     // overwrite existing value
                {
                    targetEntry.value = value;
                    return;
                }

                if (targetEntry.next is null)       // add at end of linked list
                {
                    targetEntry.next = new Entry(key, value, hashcode, null);
                    Count++;
                    ReallocateIfNeed();
                    return;
                }
                targetEntry = targetEntry.next;
            }
        }

        public bool Contains(K key)
        {
            var entry = FindEntry(key);
            if (entry is null) return false;
            return true;

        }

        public void Remove(K key)
        {
            int hashcode = key.GetHashCode();
            int targetBucket = (hashcode & int.MaxValue) % entries.Length;
            Entry? target = entries[targetBucket];
            if (target is null) return;
            while (target is not null)
            {
                if (target.hashcode == hashcode && key.Equals(target.key))
                {
                    // found Entry - so we remove it from linked list
                    target = target.next;
                    return;
                }
                target = target.next;
            }
            return;
        }

        

        // returns Value stored at key or nullvalue if not found.
        public V? GetValue(K key)
        {
            var entry = FindEntry(key);
            if (entry is null) return default(V);
            return entry.value;
        }


        /// <summary>
        /// if size reaches a certain loadfactor we reallocate with 2 times the capacity (this is really slow/expensive)
        /// </summary>
        private void ReallocateIfNeed()
        {
            const double LOADFACTOR = 0.75;
            const int GROWFACTOR = 2;
            if (Count < Capacity * LOADFACTOR) return;
            var newCapacity = Capacity * GROWFACTOR;
            var oldEntries = new List<KeyValuePair<K, V>>();
            foreach (var pair in this)
                oldEntries.Add(pair);
            entries = new Entry[newCapacity];
            foreach (var pair in oldEntries)
                this.Add(pair.Key, pair.Value);
        }

        private Entry? FindEntry(K key)
        {
            int hashcode = key.GetHashCode();
            int targetBucket = (hashcode & int.MaxValue) % entries.Length;
            Entry? target = entries[targetBucket];
            if (target is null) return null;
            while (target is not null)
            {
                if (target.hashcode == hashcode && key.Equals(target.key)) return target;
                target = target.next;
            }
            return null;
        }

        public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
        {
            var version = this.version;
            foreach(var entry in entries)
            {
                var current = entry;
                while (current is not null)
                {
                    if (this.version != version) throw new InvalidOperationException("Collection modified.");
                    yield return new KeyValuePair<K, V>(current.key, current.value);
                    current = current.next;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
