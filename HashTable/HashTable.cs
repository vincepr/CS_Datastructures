using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

/*
 * 
 * Implementation for Disjoined Set, aka a Union-Find.
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
            Console.WriteLine(table.Capacity);

        }
    }

    /// <summary>
    /// Implemention of a HashMap using SEPARATE CHAINING with Generic Key-Value pairs.
    /// </summary>
    /// <typeparam name="K">key</typeparam>
    /// <typeparam name="V">value</typeparam>
    public sealed class SimpleHashTable<K, V>
    {
        /// <summary>
        /// Represents a single key-value pair in our list
        /// </summary>
        private class Entry
        {
            public K key;
            public V value;
            public Entry next;  // to avoid using tombstones we just linked-list entries together
            public int hashcode;
        }

        /* Attributes */
        private const int START_CAPACITY = 8;
        private Entry[] entries;
        public int Count { get; private set; }
        public int Capacity => entries.Length;
        public SimpleHashTable(int startCapacity = START_CAPACITY)
        {
            startCapacity = (startCapacity > START_CAPACITY) ? startCapacity : START_CAPACITY;
            entries = new Entry[startCapacity];
        }

        /* functionality METHODS */
        public void Add(K key, V value)
        {
            int hashcode = key.GetHashCode();   // we use the default implementation of C#'s Hashing
            int targetBucket = (hashcode & int.MaxValue) % entries.Length;
            Entry targetEnt = entries[targetBucket];
            while (targetEnt is not null)
            {
                targetEnt = targetEnt.next;

            }
        }

    }
}
