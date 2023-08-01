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

## Implementation (in go)
```go
// our processes we want to queue (bigger prio -> do first)
type Process struct{
    prio int
}

// our heap structure (max heap in this case)
type Heap struct{
    arr []Process
}


// public function to add a element to the heap
func (h *Heap) Insert(proc Process){
    h.arr =  append(h.arr, proc)
    h.heapifyUp(len(h.arr)-1)
}
// bring heap back into heap-state after a Input()
// does so by swapping with parent till uptop or not bigger anymore
func (h *Heap)heapifyUp(idx int){
    for h.arr[idx].prio > h.arr[parent(idx)].prio {         // while( node>parent )
        h.swap(parent(idx), idx)
        idx = parent(idx)
    }
}


// public function to "pop()" the largest/root node
func (h *Heap) Extract() (Process, error) {
    length := len(h.arr) -1
    if length < 0 {
        return Process{}, fmt.Errorf("Heap is Empty, can not remove anything")
    }
    popElement := h.arr[0]
    h.arr[0] = h.arr[length]    // swap last element to first
    h.arr = h.arr[:length]      // remove last slice element (but does not reallocate in go if i understand correctly)

    h.heapifyDown(0)            // start our sort-shuffle from index 0
    return popElement, nil
}
// bring heap back into heap-state after a Extract()
// does so by potentially swapping with bigger child, moving down till bottom/no more swap
func (h *Heap)heapifyDown(idx int){
    current := idx
    last    := len(h.arr)-1
    l, r    := left(idx), right(idx)
    for l <= last {
        if l == last {
            current = l
        } else if h.arr[l].prio > h.arr[r].prio{
            current = l
        } else {
            current = r
        }
        if h.arr[idx].prio < h.arr[current].prio{
            h.swap(idx, current)
            idx = current
            l, r = left(idx) , right(idx)
        } else { return }
    }
}


/*
*   helpers
*/

// returns the equivalent parent/left/right node of our "thought off binary-tree"
func parent(idx int) int {
    return (idx -1) / 2
}

func left(idx int) int {
    return 2*idx +1
}

func right(idx int) int {
    return 2*idx +2
}

func (h *Heap)swap(i1 int, i2 int){
    h.arr[i1], h.arr[i2] = h.arr[i2], h.arr[i1]
}
```