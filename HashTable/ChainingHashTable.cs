using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

/*
 * 
 * Implementation for a Hash Table
 * 
 * - using Separate Chaining (with linked-list'ish chaining)
 * - null as key feels wrong to implement so i just exluded it
 * - version-Attribute used to keep track of changes after creating iterator.
 *      - we increment it every tried mutation attempt (even if that doesn't change the underlying data)
 * 
 */

namespace RS_DATASTRUCTURES.HashTable;

public static class Example
{
    public static void Run()
    {
        Console.WriteLine("--- (Simple) HashTable Example: ---");
        var table = new ChainingHashTable<int, int>(10)                   // setting starting capactiy =10
        {
            {5, 2},
            {23, 42},
        };

        for(int i = 10; i < 20; i++)
            table[i] = i*i*i;

        foreach (var pair in table)
            Console.WriteLine($"{pair}");


        Console.WriteLine("\n--- --- ---");
        // everytime the growth factor gets to high it reallocs with double size.
        Console.WriteLine($"Count: {table.Count} Capacity: {table.Capacity}");
        Console.WriteLine($"{table.Contains(10)} {table.GetValue(10)}");
        table.Remove(10);
        Console.WriteLine($"{table.Contains(10)} {table.GetValue(10)}");


        Console.WriteLine("\n--- --- ---");
        var table2 = new ChainingHashTable<String, int>(("Bob",24),("James",34),("Gandalf", 5000));
        table2["Gandalf"] = 55555;
        table2.Remove("Bob");
        foreach(var (key, value) in table2)
            Console.WriteLine($"<{key} {value}>");
        if (table2.Contains("James"))
            Console.WriteLine($"James is {table2["James"]}");
    }
}

/// <summary>
/// Implemention of a HashMap using SEPARATE CHAINING with Generic Key-Value pairs.
/// </summary>
/// <typeparam name="K">key</typeparam>
/// <typeparam name="V">value</typeparam>
public sealed class ChainingHashTable<K, V>  : IEnumerable<KeyValuePair<K, V>> where K : notnull
{
    /// <summary>
    /// Represents a single key-value pair in our list
    /// </summary>
    sealed class Entry
    {
        public K Key { get; init; }
        public V Value { get; set; }
        public int Hashcode { get; set; }
        public Entry? Next { get; set; }

        public Entry(K key, V value, int hashcode, Entry? next = null)
        {
            this.Key = key;
            this.Value = value;
            this.Hashcode = hashcode;
            this.Next = next;
        }
    }

    /* Attributes */
    private const int START_CAPACITY = 8;

    private Entry?[] entries;

    public int Count { get; private set; }

    /// <summary>
    /// version, so we can throw if Enumerator changed mid consuming.
    /// </summary>
    private int version = 0;                
    
    // Number of buckets
    public int Capacity => entries.Length;        

    public ChainingHashTable(int CAPACITY = START_CAPACITY)
    {
        CAPACITY = (CAPACITY > START_CAPACITY) ? CAPACITY : START_CAPACITY;
        entries = new Entry[CAPACITY];
    }

    public ChainingHashTable(params (K, V)[] pairs)
    {
        int capacity = (pairs.Length*2 > START_CAPACITY) ? pairs.Length*2 : START_CAPACITY;
        entries = new Entry[capacity];
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
        
        // add directly to bucket (first element)
        if (targetEntry is null)                
        {
            entries[targetBucket] = new Entry(key, value, hashcode, null);
            Count++;
            ReallocIfNeed();
            return;
        }
        // traverse the linked list:
        while (targetEntry is not null)
        {
            // overwrite existing value
            if(key.Equals(targetEntry.Key))    
            {
                targetEntry.Value = value;
                return;
            }

            // add at end of linked list
            if (targetEntry.Next is null)       
            {
                targetEntry.Next = new Entry(key, value, hashcode, null);
                Count++;
                ReallocIfNeed();
                return;
            }
            targetEntry = targetEntry.Next;
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
        version++;
        int hashcode = key.GetHashCode();
        int targetBucket = (hashcode & int.MaxValue) % entries.Length;
        Entry? previous = entries[targetBucket];
        if (previous is null) return;
        Entry? current = previous.Next;
        if (current is null) entries[targetBucket] = null;

        while (current is not null)
        {
            if (current.Hashcode == hashcode && key.Equals(current.Key))
            {
                // found Entry - so we remove it from linked list
                previous.Next = current.Next;       
                return;
            }
            previous = current;
            current = current.Next;
        }
        return;
    }

    /// <summary>
    /// returns Value stored at key or nullvalue if not found.
    /// </summary>
    public V? GetValue(K key)
    {
        var entry = FindEntry(key);
        if (entry is null) return default(V);
        return entry.Value;
    }

    /// <summary>
    /// if size reaches a certain loadfactor we reallocate with 2 times the capacity (this is slow/expensive)
    /// </summary>
    private void ReallocIfNeed()
    {
        const double LOADFACTOR = 0.75;
        const int GROWFACTOR = 2;
        if (Count < Capacity * LOADFACTOR) return;

        var newCapacity = Capacity * GROWFACTOR;
        var oldEntries = this.entries;
        this.entries = new Entry[newCapacity];
        foreach (var e in oldEntries)
        {
            if (e is not null) 
            {
                // copy roots
                this.Add(e.Key, e.Value);
                var next = e.Next;
                // copy linked elements
                while (next is not null)
                {
                    this.Add(next.Key, next.Value);
                    next = next.Next;
                }
            }
        }
    }

    private Entry? FindEntry(K key)
    {
        int hashcode = key.GetHashCode();
        int targetBucket = (hashcode & int.MaxValue) % entries.Length;
        Entry? target = entries[targetBucket];
        if (target is null) return null;
        while (target is not null)
        {
            if (target.Hashcode == hashcode && key.Equals(target.Key)) return target;
            target = target.Next;
        }
        return null;
    }

    // Inexer for our HashMap. Ex: 'var xyu = ourMap["someKey"]'
    public V? this[K key]
    {
        get => GetValue(key);
        set{ Add(key, value!); }
    }

    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    
    public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
    {
        var version = this.version;
        foreach(var entry in entries)
        {
            var current = entry;
            while (current is not null)
            {
                if (this.version != version) throw new InvalidOperationException("Collection modified.");
                yield return new KeyValuePair<K, V>(current.Key, current.Value);
                current = current.Next;
            }
        }
    }
}
