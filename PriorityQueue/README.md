# Priority Queue - (often implemented with a Heap)
Similar to normal queue BUT items of higher priority come out first.
- items need to be comparable for this. So the comparable data must be able to be sorted in some way.

## Heap (often used for priority queue)
a tree that satisfies the heap property. `If A is a parent of node B, then A is ordered with respect to B for all nodes A,B in the heap`
- Ex. if A the parent then all children and it's children are smaller.

## Usage
- can be used to implement Dijkstra's Shortest Path Algorithm
- if we for example always want to the next best (or next worst) node
    - Best First Search (BFS) often grab the next most promising node like this
- used in Huffman coding (lossless data compression)
- Minimum Spaning Tree algorithms (MST)

## Complexity
| | |
|---|---|
|Construction| O(n) |
| Polling | O(log(n)) |
| Peeking | O(1) |
| Adding| O(log(n)) |
| Naive Removing| O(n) |
| hash-table removing | O(log(n))  |
| Naive contains | O(n) |
| hash-table contains | O(1) |
- A hashtable ontop of the Heap adds overhead but makes remove() and contains() faster

```go
// implementation on md
```